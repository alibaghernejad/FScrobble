namespace FScrobble.Shell

open System
open System.Threading.Tasks
open System.Collections.Generic
open Tmds.DBus
open FScrobble.Core.Models


module Mpris =
    open FSharp.Control
    open FScrobble.Core
    open FScrobble.Core.Dependencies
    open FScrobble.Core.ReaderAsync
    open FScrobble.Core.ScrobblingFilters
    open FScrobble.Core.Constants
    open FScrobble.Core.AppConfigurations
    open System.Linq
    open FScrobble.Core.Helpers

    [<DBusInterface(MPRIS_CORE_PLAYBACK_INTERFACE)>]
    type IMediaPlayer2Player =
        inherit IDBusObject
        abstract member GetAsync<'T> : (string) -> Task<'T>
        abstract member WatchPropertiesChangedAsync: (Action<PropertyChanges>) -> Task<IDisposable>

    /// Extension-like methods for media player proxy.
    [<AutoOpen>]
    module MediaPlayer2PlayerPropertiesExtensions =
        type IMediaPlayer2Player with
            member this.GetPlaybackStatus() = this.GetAsync<string> "PlaybackStatus"
            member this.MetadataAsync = this.GetAsync<IDictionary<string, obj>> "Metadata"
            member this.Position = this.GetAsync<int64> "Position"

    let private connectToMediaPlayer (connection: Connection) (serviceName: string) =

        if String.IsNullOrWhiteSpace serviceName then
            Error "Service name must not be null or whitespace."
        elif not (serviceName.StartsWith MPRIS_ROOT_INTERFACE) then
            Error $"Service name must start with '{MPRIS_ROOT_INTERFACE}'."
        else
            try
                let proxy =
                    connection.CreateProxy<IMediaPlayer2Player>(serviceName, ObjectPath MPRIS_OBJECT_PATH)
                Ok proxy
            with ex ->
                Error $"Failed to connect to media player: {ex.Message}"
    let getScrobblingStream
        (player: IMediaPlayer2Player)
        (playerId: string)
        (streamPoller: MediaPlayerState -> option<PlaybackSnapshot> -> Async<Option<TrackInfo * DateTimeOffset * TimeSpan> * option<PlaybackSnapshot>>)=
         readerAsync{
            let! deps= ask
            let ct= deps.CancellationToken
            return
                AsyncSeq.unfoldAsync
                    (fun nowPlayingSnapshot ->
                        let rec loop acc = async {
                            if ct.IsCancellationRequested then
                                return None
                            else
                                let! status = player.GetPlaybackStatus() |> Async.AwaitTask
                                let! metadata = player.MetadataAsync |> Async.AwaitTask
                                metadata.Add(CUSTOM_PLAYER_ID,playerId)
                                let trackInfo = extractTrackInfo metadata
                                let! position = player.Position |> Async.AwaitTask
                                let! result =
                                    streamPoller
                                        {   PlaybackStatus = status
                                            TrackInfo = trackInfo
                                            Position = position }
                                        acc

                                do! Async.Sleep (int deps.Config.Scrobbling.PlaybackPollInterval.TotalMilliseconds)
                                match result with
                                | Some d,n -> return Some(d, n)
                                | None, n -> return! loop n // retry until Some
                        }
                        loop nowPlayingSnapshot)
                    None
         }
    
    let processMediaPlayerStream stream  =
        readerAsync {
        let! deps = ask
        do!
            stream 
            |> AsyncSeq.bufferByCountAndTime 10 (int deps.Config.Scrobbling.PlaybackPollInterval.TotalMilliseconds)
            |> AsyncSeq.concatSeq
            |> AsyncSeq.filterAsync  (fun d -> (scrobbleFilterWithOutbox d |> run) deps)
            |> AsyncSeq.iterAsync  (fun d -> (enqueue d |> run) deps)
        }

    let getMediaPlayerStream (playerName: string) player poller = readerAsync {
        let! (deps: AppDependencies) = ask
        deps.Log Logging.Debug $"Starting stream processing: {playerName}" None
        let tracker = deps.MediaPlayerTracker

        let cleanup () =
            readerAsync {
                try
                deps.Log Logging.Debug $"Starting Cleanup for {playerName}" None
                do! tracker.Post(Remove playerName)
                deps.Log Logging.Debug $"Finished Cleanup for {playerName}" None
                with
                | ex -> deps.Log Logging.Error $"Finished stream processing: {playerName}" (Some ex)
            }

        let! result =
            readerAsync {
                try
                    let! stream = getScrobblingStream player playerName poller
                    do! processMediaPlayerStream stream
                    return Ok ()
                with ex ->
                    return Error ex
            }

        // Run the cleanup regardless of success or failure
        do! cleanup ()

        // Re-raise if there was an error
        match result with
        | Ok () -> return deps.Log Logging.Debug $"Finished stream processing: {playerName}" None
        | Error ex -> return deps.Log Logging.Error $"Unexpected stream processing error: {playerName}" (Some ex)
    }

    let watchMediaPlayers
        (onPlayerStateChanged:
             MediaPlayerState -> option<PlaybackSnapshot> -> Async<Option<TrackInfo * DateTimeOffset * TimeSpan> * option<PlaybackSnapshot>>)
        : ReaderAsync<AppDependencies, unit> =

        readerAsync {
        let! (deps: AppDependencies) = ask
        try
            // Safely create a connection to D-Bus session bus
            let connection = new Connection(Address.Session)           
            let connectionStateChangedEventHandler (eArgs: ConnectionStateChangedEventArgs) =
                printfn $"Connection state chnaged to {eArgs.State}"
                
            connection.StateChanged.Add connectionStateChangedEventHandler
            if connection = null then
                deps.Log Logging.Error "Failed to create DBus connection." None
            else 
                // Can only connect once
                let! connectionInfo = connection.ConnectAsync() |> Async.AwaitTask
                deps.Log Logging.Information $"Connected to Localname:{connectionInfo.LocalName}, RemoteName: {connectionInfo.RemoteIsBus}" None


            /// Checks whether a given MPRIS player D-Bus name is still active
            let isPlayerActive (playerName: string) : Async<bool> =
                async {
                    try
                        let! isActive =
                            connection.IsServiceActiveAsync playerName |> Async.AwaitTask
                        return isActive
                    with ex ->
                        printfn "Error checking player '%s': %s" playerName ex.Message
                        return false
                }

            
            let getMediaPlayerNames 
                (isFirstIter:bool, 
                maxPlayers: int, 
                tracker:EventBus<MediaPlayerTrackerMsg>,
                deps:AppDependencies) =
                async {
                    let! names = connection.ListServicesAsync() |> Async.AwaitTask
                    let namesFiltered = names |> Array.filter (fun name -> name.StartsWith(MPRIS_ROOT_INTERFACE))
                    if not (namesFiltered.Any()) then
                        printfn "No running media player detected."
                        printfn "Start your favotite players like 'Musikcube', 'Spotify', 'YouTube Music Web', ..."
                    let! results =
                        [ for p in namesFiltered -> async {
                            let! isActivable = isPlayerActive p
                            let isAllowedPlayer = isAllowedPlayer (getPlayerId p) (List.ofArray deps.Config.Scrobbling.AllowedPlayers)
                            let! result =  
                                match isAllowedPlayer, isActivable, tracker with
                                | true,true, _ -> async{
                                    let! isNew =  tracker.PostAndAsyncReply(fun reply -> TryAdd(p, reply))
                                    if not isNew then
                                        printfn "Discard player: '%s' Already Processing."  p
                                    return Option.ofBool isNew |> Option.map (fun _ -> p)  }
                                | false,_,_-> 
                                    printfn "Discard player: '%s' Not Allowed."  p
                                    async.Return None
                                | _ -> async.Return None
                            return result
                            // return if isNew then Some p else None
                        } ]
                        |> Async.Parallel

                    let resultsSomes = 
                        results
                        |> Array.choose id

                    return Some(resultsSomes, (false,maxPlayers))
                }

            let getMediaPlayers tracker (deps:AppDependencies): AsyncSeq<string array> =
                AsyncSeq.unfoldAsync (fun (isFirst,acc) ->
                    async {
                        if deps.CancellationToken.IsCancellationRequested then
                            return ()
                        if not isFirst then
                            do! Async.Sleep (int deps.Config.Scrobbling.MprisServicesPollInterval.TotalMilliseconds)
                        let! result = getMediaPlayerNames (isFirst, acc, tracker, deps)
                        return result
                    }
                ) (true,3)
            
            let getPlayersStreams (playerNames:AsyncSeq<string>) =
                asyncSeq {
                    for playerName in playerNames do
                        let player = connectToMediaPlayer connection playerName
                        match player with
                        | Ok player ->
                            // Add player to watching List
                            yield (playerName, player)
                        | Error err ->
                            //Ignore the player.
                            printfn "Error connecting to media player %s: %s" playerName err
                }
            let filterAllowedPlayers config =
                AsyncSeq.filter (defaultAllowedPlayerFilter config)

            let processStream =
                fun (playerName, player) ->
                let stream = getMediaPlayerStream playerName player onPlayerStateChanged
                run stream deps

            let tracker = deps.MediaPlayerTracker
            
            do!
            getMediaPlayers tracker deps
            |> AsyncSeq.concatSeq
            |> filterAllowedPlayers deps.Config.Scrobbling
            // get player streams
            |> getPlayersStreams
            |> AsyncSeq.groupBy ( fun e -> fst e)
            // Parallel processing for multiple player instances
            |> AsyncSeq.mapAsyncParallel (snd >> AsyncSeq.iterAsync processStream )

            |> AsyncSeq.iter ignore
        with
        | e -> deps.Log Logging.Information e.Message None
        }