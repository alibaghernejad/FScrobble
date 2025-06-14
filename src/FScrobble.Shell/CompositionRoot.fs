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
open FScrobble.Core.Messaging

let buildAppDependencies (cfg: IConfiguration, logger: ILogger, ct:CancellationToken) : AppDependencies =
    let log = createLogger logger
    let config = ConfigLoader.load2 cfg
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

    let finalEnv = { deps with EventBus = eventBus; MediaPlayerTracker = tracker }

    // envRef <- deps
    finalEnv

