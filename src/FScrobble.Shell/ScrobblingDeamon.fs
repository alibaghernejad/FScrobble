namespace FScrobble.Core

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open FScrobble.Core.Logging
open FSharp.Control
open FScrobble.Core.Models
open FScrobble.Shell.Logging
open FScrobble.Shell.Mpris
    type TrackPositionState = {
    TrackId: string
    LastPosition: TimeSpan
    LoopCount: int
    LastTimestamp: DateTime
    LastLoopDetectedAt: DateTimeOffset option
}

type PlaybackState = {
    LastPosition: TimeSpan
    LastLoopDetectedAt: DateTimeOffset option
}

type LoopDetectionResult =
    | LoopDetected of PlaybackState
    | NoLoop of PlaybackState

type ScrobblingDeamon(logger: ILogger<ScrobblingDeamon>, configuration: IConfiguration) =
    inherit BackgroundService()

    let log = createLogger logger
    let mutable currentState : PlaybackSnapshot option = None
    let mutable (l:LoopDetectionResult) = NoLoop { LastPosition = TimeSpan.Zero; LastLoopDetectedAt= None}
    let shouldScrobble (track: TrackInfo) (startTime: DateTimeOffset) = true
    let mutable currentState : TrackPositionState option = None
    let detectLoop 
        (position: TimeSpan) 
        (state: PlaybackState) 
        (trackDuration: TimeSpan) 
        (now: DateTimeOffset)
        : LoopDetectionResult =

        let delta = position - state.LastPosition
        let cooldown = TimeSpan.FromSeconds 20.0

        let deltaThreshold = trackDuration.TotalSeconds * 0.10
        let startThreshold = trackDuration.TotalSeconds * 0.10
        let minPlayedThreshold = trackDuration.TotalSeconds * 0.50

        let recentlyDetected =
            match state.LastLoopDetectedAt with
            | Some ts -> now - ts < cooldown
            | None -> false

        let isLooping =
            not recentlyDetected &&
            delta.TotalSeconds < -deltaThreshold &&
            position.TotalSeconds < startThreshold &&
            state.LastPosition.TotalSeconds > minPlayedThreshold

        let r = 
            if isLooping then
                printfn "LOOOOOOOP"
                // Update loop time and last position
                let newState = {
                    state with 
                        LastLoopDetectedAt = Some now
                        LastPosition = position
                }
                LoopDetected newState
            else
                printfn "XXXXXXXXX"
                // Just update position
                let newState = { state with LastPosition = position }
                NoLoop newState
        r

    let getValidEffectiveTime (state:TrackInfo) (effectiveTime:TimeSpan) =
        match effectiveTime, state.Length with
        | t, Some l when t >= l -> l
        |_ -> effectiveTime

    let pollMediaPlayer
        (state: MediaPlayerState)
        (acc: option<PlaybackSnapshot>)
        : Async<(Option<TrackInfo * DateTimeOffset * TimeSpan> * PlaybackSnapshot option)> =

        async {
            printfn "Player state poll: %A, %A" state acc 
            // let now = DateTimeOffset.UtcNow
            let getSnapshot track timestamp ept playerState acc2=
                Some{ 
                    CurrentTrack = track; 
                    PlaybackStartTime = timestamp; 
                    Position= 0(* state.Position *);  
                    EffectivePlayTime= ept
                    MediaPlayerState= playerState
                    AccumulatedCalc2= acc2}
            
           
            let isTransitionToTheEndOfPlayback now before : bool =
                match now, before with
                | ("Paused" | "Stopped"), "Playing" -> true
                | _ -> false

            let isResumePlayback now before : bool =
                match now, before with
                | "Playing", ("Paused"|"Stopped") -> true
                | _ -> false
                    
            // add is loop detected with new id
            let result = 
                match state.TrackInfo, state.PlaybackStatus, acc with
                    // New track started playing.
                    // First ever track started.
                    | Some nowPlaying, _, None -> 
                        None , getSnapshot nowPlaying DateTimeOffset.UtcNow TimeSpan.Zero state (DateTimeOffset.UtcNow,TimeSpan.Zero)

                    // Continue playing same track
                    // Maybe No action needed, just continue ir scrobble reached
                    | (Some nowPlaying), "Playing" , Some before when nowPlaying.Id = before.CurrentTrack.Id-> 
                        // Causse gen new id infini tloop?
                        // ad to accu,ulator and check the result
                        //  if LoopDetected... then
                        //     {t with ReplayCount=2, id=regenrateid}
                                // regenerate id
                        let offset, ac = 
                            if isResumePlayback state.PlaybackStatus before.MediaPlayerState.PlaybackStatus then 
                                DateTimeOffset.UtcNow, snd before.AccumulatedCalc2
                            else
                                before.AccumulatedCalc2
                        
                        let effectiveTime =
                            ac.Add(DateTimeOffset.UtcNow - offset) 
                            |> getValidEffectiveTime before.CurrentTrack
                        let snapshot = Some {before with CurrentTrack= nowPlaying; MediaPlayerState=state;  EffectivePlayTime = effectiveTime; AccumulatedCalc2=offset,ac; }
                        Some(before.CurrentTrack, before.PlaybackStartTime, before.EffectivePlayTime), snapshot 
        
                    // Start tracking new track.
                    // The now playing track is a new track in Playing state.
                    | Some nowPlaying, "Playing" , Some before when nowPlaying.Id <> before.CurrentTrack.Id -> 
                        Some(before.CurrentTrack, before.PlaybackStartTime, before.EffectivePlayTime), getSnapshot nowPlaying DateTimeOffset.UtcNow TimeSpan.Zero state (DateTimeOffset.UtcNow,TimeSpan.Zero)
                    
                    // Track ended (paused/stopped) 
                    // The previous track is the same as current
                    | Some nowPlaying, ("Paused" | "Stopped") , Some before when nowPlaying.Id = before.CurrentTrack.Id->  
                        let isTransitionToTheEnd = 
                            isTransitionToTheEndOfPlayback state.PlaybackStatus before.MediaPlayerState.PlaybackStatus
                        let offset, ac = 
                            if isTransitionToTheEnd then
                                fst before.AccumulatedCalc2,(snd before.AccumulatedCalc2)
                                            .Add(DateTimeOffset.UtcNow - fst before.AccumulatedCalc2)
                            else
                                before.AccumulatedCalc2
                        let effectiveTime =
                            match isTransitionToTheEnd with 
                            | true -> ac 
                            | _ -> before.EffectivePlayTime
                            |> getValidEffectiveTime before.CurrentTrack

                        let snapshot = Some {before with  CurrentTrack= nowPlaying; MediaPlayerState=state; EffectivePlayTime = effectiveTime; AccumulatedCalc2= offset,ac}
                        Some(before.CurrentTrack, before.PlaybackStartTime, effectiveTime), snapshot 

                    // New Detection on the pause/Stopped states.
                    // The now playing track is new but paused/stopped.
                    | Some nowPlaying, ("Paused" | "Stopped") , Some before when nowPlaying.Id <> before.CurrentTrack.Id->  
                        let snapshot = getSnapshot nowPlaying DateTimeOffset.UtcNow TimeSpan.Zero state (DateTimeOffset.UtcNow,TimeSpan.Zero)
                        Some(before.CurrentTrack, before.PlaybackStartTime,  before.EffectivePlayTime),snapshot 
                    
                    // Any other Sate
                    | _ -> 
                        None, None
    
            return result

        }

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            try
                let deps = CompositionRoot.buildAppDependencies(configuration, logger, ct)

                let longRunningTask = watchMediaPlayers pollMediaPlayer
                let r = ReaderAsync.run longRunningTask deps
                do! r |> Async.StartAsTask

                while not ct.IsCancellationRequested do
                    do! Task.Delay(1000, ct)

            with ex ->
                log Error (sprintf "Error in %s service" <| nameof ScrobblingDeamon) (Some ex)
                return raise ex
        }

        
