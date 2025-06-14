namespace FScrobble.Core

module Messaging =
    open System.Collections.Generic
    open FScrobble.Core.Models

    let andyYellowtailMediaPlayerInFlightInspector () =
        MailboxProcessor.Start(fun inbox ->
            let inFlight = HashSet<string>()

            let rec loop () =
                async {
                    let! msg = inbox.Receive()

                    match msg with
                    | TryAdd(instanceId, reply) ->
                        let added = inFlight.Add(instanceId)
                        reply.Reply(added)
                        return! loop ()
                    | Remove(instanceId) ->
                        inFlight.Remove(instanceId) |> ignore
                        return! loop ()
                }

            loop ())
