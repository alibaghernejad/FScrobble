module CompositionRoot

open FScrobble.Core.Dependencies
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open FScrobble.Core.Outbox
open FScrobble.Shell.Logging
open FScrobble.Shell.Outbox
open FScrobble.Shell
open FScrobble.Shell.MessageQueue
open FScrobble.Core.Models
open System
open System.Threading
open FScrobble.Core.Logging

let buildAppDependencies (cfg: IConfiguration, logger: ILogger, ct:CancellationToken) : AppDependencies =
    let log = createLogger logger
    let config = ConfigLoader.load cfg
    let outbox = InMemoryScrobbleOutbox() :> IScrobbleOutbox
    let mutable envRef : EventBus<TrackInfo*DateTimeOffset> option = None

    let deps = 
      { Log = log
        Config = config
        Outbox = outbox
        EventBus = Unchecked.defaultof<_> 
        MediaPlayerTracker = Unchecked.defaultof<_> 
        CancellationToken = ct}
    let eventBus = createWithEnv handlerBuilder deps
    let tracker = createTrackerWithEnv deps

    deps.Log Information $"Using configuration: {config}" None
    let finalEnv = { deps with EventBus = eventBus; MediaPlayerTracker = tracker }

    // envRef <- deps
    finalEnv

