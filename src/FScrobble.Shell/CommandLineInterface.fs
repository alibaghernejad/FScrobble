namespace FScrobble.Shell

open System

// This module provides a command line interface for the FScrobble application.
// It parses command line arguments, validates them, and prepares options for the application.
// It also includes an entry point for running the application from the command line.
module CommandLineInterface =
    open FScrobble.Core.ReaderAsync
    open FScrobble.Core.Dependencies
    open FScrobble.LastFm
    open FScrobble.Core.Constants
    open FScrobble.Core.Helpers

    type ScrobbleServers =
        | LastFm of string
        | LibreFm of string

    type CommandLineOptions =
        { LastFmApiKey: string
          LastFmApiSecret: string
          LastFmSessionKey: string
          MprisPlayerName: string
          ConnectScrobblingServer: string }

    let parseCommandLineArgs args =
        let rec parse acc =
            function
            | [] -> acc
            // Uncomment to add more command and options
            // | "--lastfm-api-key" :: value :: tail -> parse { acc with LastFmApiKey = value } tail
            // | "--lastfm-api-secret" :: value :: tail -> parse { acc with LastFmApiSecret = value } tail
            // | "--lastfm-session-key" :: value :: tail -> parse { acc with LastFmSessionKey = value } tail
            // | "--mpris-player-name" :: value :: tail -> parse { acc with MprisPlayerName = value } tail
            | "connect" :: value :: tail ->  parse { acc with ConnectScrobblingServer = value } tail
            | _ -> failwith "Invalid command line arguments"

        parse
            { LastFmApiKey = ""
              LastFmApiSecret = ""
              LastFmSessionKey = ""
              MprisPlayerName = MPRIS_CORE_PLAYBACK_INTERFACE
              ConnectScrobblingServer = "" }
            (List.ofArray args)

    let validateOptions options =       
        if String.IsNullOrWhiteSpace options.MprisPlayerName then
            printfn "MPRIS player name is required."

        options

    let getCommandLineOptions args =
        args
        |> parseCommandLineArgs
        |> validateOptions
        |> fun options ->
            printfn "Parsed command line options: %A" options
            options

    let connectLastFmServer () : ReaderAsync<AppDependencies, Result<string,ScrobbleError>> = 
        readerAsync {
            let! res = getSessionKeyOrAuthenticate ()  
            match res with
            | Ok sessionKey           -> return Ok sessionKey
            | Error err -> return Error err
        }

    let runCommandLineInterface (args) : ReaderAsync<AppDependencies,unit> =
        readerAsync{
            try
                let! (deps) = ask
                let options = getCommandLineOptions args
                printfn "Running application with options: %A" options
                match normalize options.ConnectScrobblingServer with
                | "lastfm" -> 
                    let! res = connectLastFmServer ()
                    match res with
                    | Ok sessionKey ->
                        printfn "Connection to %s scrobbling Server initialized with success" options.ConnectScrobblingServer
                        printfn "Close the program and start it again in normal mode."
                    | Error err ->
                        printfn "Failed to connect to %s scrobbling Server. %A" options.ConnectScrobblingServer err     
                | _ -> printfn "Not Supported Scrobbling Server. valid Servers: %A" [nameof LastFm]
            with ex ->
                printfn "Error: %s" ex.Message
                Environment.Exit(1)
        }

