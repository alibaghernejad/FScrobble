namespace FScrobble.Shell

module MessageQueue =
    open FScrobble.Core.ReaderAsync
    open System
    open FScrobble.Core.Models
    open FScrobble.Core.Dependencies
    open FScrobble.Core.Messaging

    /// The message processing function
    let LastFmScrobblingProcessor
        (deps: AppDependencies)
        (inbox: MailboxProcessor<TrackInfo * DateTimeOffset>)
        (msg:TrackInfo*DateTimeOffset)
        : Async<unit> =
        async{
            let (ReaderAsync scrobbleRunner) = LastFmScrobbler.scrobbleTrack (fst msg) (snd msg)
            do! scrobbleRunner deps
        
        }
            
    let handlerBuilder
        (env: AppDependencies)
        (_agent: MailboxProcessor<TrackInfo * DateTimeOffset>)
        (msg: TrackInfo * DateTimeOffset)
        =
        async { do! LastFmScrobblingProcessor env _agent msg }

    /// Factory that creates the event bus.
    let createWithEnv<'msg, 'reply>
        (handlerBuilder: AppDependencies -> MailboxProcessor<'msg> -> 'msg -> Async<unit>)
        (env: AppDependencies)
        : EventBus<'msg> =

        let agent =
            MailboxProcessor.Start(fun inbox ->
                let rec loop () =
                    async {
                        let! msg = inbox.Receive()
                        do! handlerBuilder env inbox msg
                        return! loop ()
                    }

                loop ())
        { Post = fun msg -> async { agent.Post msg }
          PostAndAsyncReply = fun msg -> async {return! agent.PostAndAsyncReply msg }}


    /// Factory that creates the event bus.
    let createTrackerWithEnv
        (env: AppDependencies)
        : EventBus<MediaPlayerTrackerMsg> =
        let agent = andyYellowtailMediaPlayerInFlightInspector()
        { Post = fun msg -> async { agent.Post msg }
          PostAndAsyncReply = fun msg -> async {return! agent.PostAndAsyncReply msg }}