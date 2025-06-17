namespace FScrobble.Core
module ScrobblingFilters =

    open System
    open FScrobble.Core.Models
    open FScrobble.Core.ReaderAsync
    open FScrobble.Core.AppConfigurations
    open FScrobble.Core.Dependencies
    open FScrobble.Core.Outbox
    open FScrobble.Core
    open FScrobble.Core.Helpers

    // Calculate the scrobbling time using the hybrid fractional/fixed time
    let getScrobbleTime (track:TrackInfo ) (config:Scrobbling) =
        match track.Length with
        | Some length -> min (length.TotalSeconds * config.ThresholdFraction) config.ThresholdFixed.TotalSeconds
        | _ -> config.ThresholdFixed.TotalSeconds

    // Calculate the scrobbling time using the hybrid fractional/fixed time
    let hasMinimumRequirementPolicy effectivePlaybackTime (config:Scrobbling) =
        effectivePlaybackTime >= config.ThresholdMin

    /// Determines whether a track should be scrobbled AND hasn't been already
    let isTrackScrobbleWorthy (config: Scrobbling) (track: TrackInfo, startTime: DateTimeOffset, effectivePlayTime: TimeSpan) =
        // let effectiveDuration= DateTimeOffset.UtcNow - startTime
        let effectiveDuration= effectivePlayTime
        match track.Length, effectiveDuration with
        //For tracks with known length, consider the fractional and fixed threshold both.
        | Some _, played  ->
            hasMinimumRequirementPolicy played config &&
            played.TotalSeconds >= getScrobbleTime track config
        //For tracks with unknows length, consider the fixed threshold only.
        | None, played ->
            hasMinimumRequirementPolicy played config &&
            played.TotalSeconds >= getScrobbleTime track config
        
    /// After a successful scrobble, commit it to outbox
    let commitScrobble (track: TrackInfo, startTime: DateTimeOffset) (outbox:IScrobbleOutbox) : Async< unit> = async {
        let entry = {
            PlayerId = track.PlayerId
            TrackId = track.Id
            PlayedAt = startTime
            ScrobbledAt = DateTimeOffset.UtcNow
        }
        do! outbox.Save entry
    }
    
    /// The main filtering function with idempotency support
    let scrobbleFilterWithOutbox (track: TrackInfo, startTime: DateTimeOffset, effectivePlayTime: TimeSpan) = readerAsync {
        let! deps = ask
        let config = deps.Config.Scrobbling
        let outbox = deps.Outbox
        let log = deps.Log

        let mapper = track.MetadataMap
        let isMetadataRulesOK = isAllowedPlayerMetadata (getPlayerId track.PlayerId) mapper (List.ofArray config.AllowedPlayers)
        let isWorthy = isTrackScrobbleWorthy config (track, startTime, effectivePlayTime)
        
        if isMetadataRulesOK && isWorthy then
            let! alreadyExists = outbox.Exists(track.PlayerId, track.Id, startTime)
            if not alreadyExists then
                do! commitScrobble (track, startTime) outbox
            return not alreadyExists
        else
            log Logging.Debug $"ScrobbleFilterCheck: Track {track.Id} is isworthy= {isWorthy}, isMetadataRulesOK= {isMetadataRulesOK} detected." None
            return false
    }

        
    /// After a successful scrobble, write to outbox
    let enqueue (track: TrackInfo, startTime: DateTimeOffset, effectivePlayTime: TimeSpan) : ReaderAsync<AppDependencies, unit> = readerAsync {
        
        let! (deps:AppDependencies) = ask
        let log = deps.Log
        let eventBus = deps.EventBus
        log Logging.Information $"Enqueueing Track with id {track.Id}, 
            startTime={startTime}, effectivePlayTime={effectivePlayTime}" None
        do! eventBus.Post(track,startTime)
        log Logging.Information $"Posted Track with id {track.Id}" None

    }

    /// Check if a player isamoung the allowed media players or not.
    let defaultAllowedPlayerFilter (config:AppConfigurations.Scrobbling) (serviceName:string)  =    
        let rootlessPlayername = getRootlessPlayerName serviceName
        config.AllowedPlayers
        |>
        Array.exists (fun pattern -> rootlessPlayername.Contains( pattern.Name.Replace("*", "")))
        // config.AllowedPlayers.Any (fun p -> p.Name= serviceName)
        // &&
        // Any other condition
