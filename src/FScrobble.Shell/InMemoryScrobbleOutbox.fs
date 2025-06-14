namespace FScrobble.Shell

module Outbox =
    open System.Collections.Concurrent
    open System
    open FScrobble.Core.Outbox
    open FScrobble.Core.ConcurrentDictionaryExtensions

    type InMemoryScrobbleOutbox() =
        // An store for head/latest items per player.
        let store = ConcurrentDictionary<string, string>()

        let isSameAsLast (player: string) (track: string) =
            match store.TryGetValue(player) with
            | true, lastTrack -> lastTrack = track
            | _ -> false

        let getKeys playerId trackId =
            let playerKey = $"{playerId}::"
            let trackKey = $"{trackId}::"
            playerKey, trackKey



        interface IScrobbleOutbox with
            member _.Exists(playerId, trackId, playedAt) =
                let pk, tk = getKeys playerId trackId
                async.Return(isSameAsLast pk tk)

            member _.Save(entry) =
                let pk, tk = getKeys entry.PlayerId entry.TrackId
                let result = store.tryAddOrUpdate pk tk
                async.Return()
