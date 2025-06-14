namespace FScrobble.Core

module Logging =

    type LogSeverity =
        | Trace
        | Debug
        | Information
        | Warning
        | Error
        | Critical
        | Off

    type Log = LogSeverity -> string -> exn option -> unit
