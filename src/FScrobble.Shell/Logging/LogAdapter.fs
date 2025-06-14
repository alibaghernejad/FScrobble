namespace FScrobble.Shell

module Logging =
    open FScrobble.Core.Logging
    open Microsoft.Extensions.Logging

    let toLogLevel =
        function
        | Trace -> LogLevel.Trace
        | Debug -> LogLevel.Debug
        | Information -> LogLevel.Information
        | Warning -> LogLevel.Warning
        | Error -> LogLevel.Error
        | Critical -> LogLevel.Critical
        | _ -> LogLevel.None

    let logAdapter (logger:ILogger): Log =
        fun severity msg ex ->
            match severity, msg, ex with
            | Off, _, _ -> ()
            | _, m, None -> logger.Log(toLogLevel severity, m)
            | _ -> logger.Log(toLogLevel severity, ex.Value, msg)

    let createLogger<'T when 'T :> ILogger> (logger: 'T) : Log =
        logAdapter logger