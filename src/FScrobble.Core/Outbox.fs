namespace FScrobble.Core

module Outbox =
    open System
    
    type OutboxEntry = {
        TrackId: string
        PlayerId: string
        PlayedAt: DateTimeOffset
        ScrobbledAt: DateTimeOffset
    }

    type IScrobbleOutbox =
        abstract member Exists: playerId:string * trackId:string * playedAt:DateTimeOffset -> Async<bool>
        abstract member Save: OutboxEntry -> Async<unit>
    