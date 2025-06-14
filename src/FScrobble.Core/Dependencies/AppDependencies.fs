namespace FScrobble.Core.Dependencies

open FScrobble.Core.Logging
open FScrobble.Core.AppConfigurations
open FScrobble.Core.Outbox
open FScrobble.Core.Models
open System
open System.Threading


type AppDependencies =
    { Log: Log
      Config: AppConfigurations
      Outbox: IScrobbleOutbox
      EventBus: EventBus<TrackInfo * DateTimeOffset>
      MediaPlayerTracker : EventBus<MediaPlayerTrackerMsg>
      CancellationToken: CancellationToken
       }
