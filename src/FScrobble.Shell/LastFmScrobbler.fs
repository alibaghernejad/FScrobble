module LastFmScrobbler

open System
open FScrobble.Core.Models
open FScrobble.LastFm
open FScrobble.Core.ReaderAsync
open FScrobble.Core.Dependencies
module l = FScrobble.Core.Logging

let getLastFmTrack (track: TrackInfo) (startTime: DateTimeOffset) =
    { Track.Artist = String.Join(", ", track.Artist)
      Track.Title = track.Title
      Track.Album = track.Album
      Track.Duration = track.Length
      Track.Timestamp = startTime }

let scrobbleTrack (track: TrackInfo) (startTime: DateTimeOffset)  : ReaderAsync<AppDependencies, unit> =
    readerAsync {
        let! deps = ask
        let log = deps.Log
        let (ReaderAsync sessionKeyReader) = getSessionKeyOrAuthenticate ()
        let! sessionKeyResult = sessionKeyReader deps

        match sessionKeyResult with
        | Ok sessionKey ->
            let scrobbleTrackEntry = getLastFmTrack track startTime

            let credentials =
                { ApiKey = deps.Config.LastFm.ApiKey
                  ApiSecret = deps.Config.LastFm.ApiSecret
                  SessionKey = sessionKey }

            let (ReaderAsync scrobbleRunner) = scrobbleTrack scrobbleTrackEntry credentials
            let! result = scrobbleRunner deps

            match result with
            | Ok() ->
                log l.Information $"""Scrobbled: {track.Title} by {String.Join(",", track.Artist)}""" None

            | Error error ->
                match error with
                | NetworkError msg -> log l.Error "Network error while scrobbling: {msg}" None
                | ApiError(code, msg) -> log l.Error "Last.fm API error {code}: {msg}" None
                | AuthenticationError ->
                    log l.Error "Authentication failed. Please check your Last.fm credentials." None
                | UnexpectedError ex -> log l.Error "Unexpected error occurred: {ex}" (Some ex)
        | Error err -> log l.Error $"Failed to retrieve session key: {err} " None
    }
