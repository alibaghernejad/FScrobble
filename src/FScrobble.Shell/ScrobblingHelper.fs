module ScrobblingHelper

    open System
    open FScrobble.Core.Models
    

    // let filterScrobbles (scrobbles: Track list) (startTime: DateTimeOffset) =
    //     scrobbles
    //     |> List.filter (fun track ->
    //         match track.Duration with
    //         | Some length when length > TimeSpan.FromMinutes(8.0) ->
    //             // For long tracks (>8 min), scrobble after 4 minutes
    //             DateTimeOffset.Now - startTime > TimeSpan.FromMinutes(4.0)
    //         | Some length ->
    //             // For normal tracks, scrobble after 50% of duration
    //             DateTimeOffset.Now - startTime > (length / 2.0)
    //         | None ->
    //             // If length unknown, use 30 seconds rule
    //             DateTimeOffset.Now - startTime > TimeSpan.FromSeconds(30.0))


    // let ShortTracksFilter (track: TrackInfo) =
    //     async{

    //         match track.Length with
    //         | Some length when length < TimeSpan.FromMinutes(2.0) -> return true
    //         | _ -> return false
    //     }



    //     // let playDuration = DateTimeOffset.Now - startTime
    //     // match track.Length with
    //     // | Some length when length > TimeSpan.FromMinutes(8.0) ->
    //     //     // For long tracks (>8 min), scrobble after 4 minutes
    //     //     playDuration > TimeSpan.FromMinutes(4.0)
    //     // | Some length ->
    //     //     // For normal tracks, scrobble after 50% of duration
    //     //     playDuration > (length / 2.0)
    //     // | None ->
    //     //     // If length unknown, use 30 seconds rule
    //     //     playDuration > TimeSpan.FromSeconds(30.0)