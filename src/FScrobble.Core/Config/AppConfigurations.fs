namespace FScrobble.Core

module AppConfigurations =
    open System
    open System.Collections.Generic

    [<CLIMutable>]
    type LogLevel =
        { Default: string
          ``Microsoft.Hosting.Lifetime``: string }

    [<CLIMutable>]
    type Logging = { LogLevel: LogLevel }

    [<CLIMutable>]
    type AllowedPlayer =
        { Name: string
          Metadata: Dictionary<string, string []>  }
          
    [<CLIMutable>]
    type Scrobbling =
        { PlaybackPollInterval: TimeSpan
          MprisServicesPollInterval: TimeSpan
          AllowedPlayers: AllowedPlayer []
          ThresholdFixed: TimeSpan
          ThresholdFraction: float
          ThresholdMin: TimeSpan }

    [<CLIMutable>]
    type LastFm =
        { ApiKey: string
          ApiSecret: string
          SessionKey: string
          BaseUrl: string
          IsEnabled: bool }

    [<CLIMutable>]
    type LibreFm =
        { ApiKey: string
          BaseUrl: string
          IsEnabled: bool }

    [<CLIMutable>]
    type AppConfigurations =
        { Logging: Logging
          Scrobbling: Scrobbling
          LastFm: LastFm
          LibreFm: LibreFm
        // MusicBrainz: MusicBrainz
        }

