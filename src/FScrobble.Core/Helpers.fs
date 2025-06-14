namespace FScrobble.Core


module Helpers =
    open FScrobble.Core.Models
    open System
    open System.Security.Cryptography
    open System.Text
    open System.Collections.Generic;
    open Constants
    open AppConfigurations

    let normalize (s: string) =
        s.Trim().ToLowerInvariant()

    let hashString (input: string) =
        using (SHA256.Create()) (fun sha ->
            input
            |> Encoding.UTF8.GetBytes
            |> sha.ComputeHash
            |> Convert.ToBase64String
        )

    let getTrackId (track: TrackInfo) : string =
        match track with
        // If MusicBrainz ID or external ID is available (extend TrackInfo if needed)
        // | Some id when not (String.IsNullOrWhiteSpace id) -> id

        | _ ->
            let artist = defaultArg (Some track.Artist) [DEFAULT_ARTIST]
            let title = defaultArg (Some track.Title) DEFAULT_TITLE
            let album = defaultArg track.Album DEFAULT_ALBUM
            let url = defaultArg track.Url DEFAULT_URL
            let duration = 
                match track.Length with
                | Some d -> d.TotalSeconds.ToString("F0")
                | None -> DEFAULT_DURATION

            let raw = $"""{normalize (String.Join(", ", artist))}|{normalize title}|{normalize url}|{normalize album}|{duration}"""
            hashString raw


    let extractTrackInfo (metadata: IDictionary<string, obj>) =
        try
            let getStringValue (key: string) =
                match metadata.TryGetValue(key) with
                | true, value -> Some(value.ToString())
                | _ -> None

            let getStringArrayValue (key: string) =
                match metadata.TryGetValue(key) with
                | true, value ->
                    match value with
                    | :? (string[]) as arr -> Array.toList arr
                    | _ -> []
                | _ -> []

            let getMicrosecondsValue (key: string) =
                match metadata.TryGetValue(key) with
                | true, value ->
                    match Int64.TryParse(value.ToString()) with
                    | true, microseconds -> Some(TimeSpan.FromMilliseconds(float microseconds / 1000.0))
                    | _ -> None
                | _ -> None

            let artist = getStringArrayValue XESAM_ARTIST
            let title = getStringValue XESAM_TITLE
            let album = getStringValue XESAM_ALBUM
            let length = getMicrosecondsValue MPRIS_LENGTH
            let url = getStringValue XESAM_URL
            let playerId = getStringValue CUSTOM_PLAYER_ID

            let metadataMapper = 
                Map.ofList [
                    XESAM_ARTIST, defaultArg (getStringValue XESAM_ARTIST) DEFAULT_ARTIST
                    XESAM_TITLE, defaultArg title DEFAULT_TITLE
                    XESAM_ALBUM, defaultArg album DEFAULT_ALBUM
                    XESAM_URL, defaultArg url DEFAULT_URL
                ]
            
            match title with
            | Some title ->
                let trackinfo = 
                    { Id = ""
                      Artist = if List.isEmpty artist then [ DEFAULT_ARTIST ] else artist
                      Title = title
                      Album = album
                      Length = length
                      Url= url
                      ReplayCount =0us
                      PlayerId = Option.defaultValue DEFAULT_PLAYER playerId
                      MetadataMap = metadataMapper } 
                Some {trackinfo with Id= getTrackId trackinfo}
            | None -> None
        with _ ->
            None

    let getBasePlayerName (serviceName: string) : string =
        serviceName
        |> fun name ->
            if name.StartsWith(MPRIS_INTERFACE_PREFIX) then
                name.Substring(MPRIS_INTERFACE_PREFIX.Length)
            else name
        |> fun name -> name.Split('.') |> Array.head
// "org.mpris.mediaplayer2.chromium.instance20438"
//  org.mpris.MediaPlayer2.

    let getRootlessPlayerName (serviceName: string) : string =
        serviceName
        |> normalize
        |> fun name ->
            if name.StartsWith(normalize MPRIS_INTERFACE_PREFIX) then
                name.Substring(MPRIS_INTERFACE_PREFIX.Length)
            else name
        
    let getPlayerId (serviceName: string) : string =
        serviceName
        |> normalize
        |> getRootlessPlayerName 

    /// Convert enumerables like Dictionary to Map
    let inline toMap v =
        match v with
        | null -> Map.empty
        | _ -> v
            |> Seq.map (|KeyValue|)
            |> Map.ofSeq

    // Media players metadata rules matching
    let isAllowedPlayerMetadata (playerName: string) (metadata: Map<string, string >) (rules: AllowedPlayer list) =
          rules |> List.exists (fun rule ->
            if (normalize playerName).Contains (normalize (rule.Name.Replace("*", "")))   then
                match toMap rule.Metadata with
                | m when m.IsEmpty -> true
                | metaRules ->
                    metaRules
                    |> Map.forall (fun key patterns ->
                        match metadata.TryFind key with
                        | Some value ->
                            patterns  |> Array.exists (fun pattern -> value.Contains( pattern.Replace("*", "")))
                        | None -> false
                    )
            else false
        )


module Option =
    let ofBool b = if b then Some () else None


/// Extension-like methods for media player proxy.
[<AutoOpen>]
module ConcurrentDictionaryExtensions =
    open System.Collections.Concurrent
    type ConcurrentDictionary<'K,'V> with
        member this.tryAddOrUpdate k v = 
            match this.TryGetValue k with
            | true, value -> this.TryUpdate(k,v, value)
            | _ -> this.TryAdd(k,v)



