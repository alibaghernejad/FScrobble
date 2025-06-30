namespace FScrobble.Core.Configurations

module Defaults =
    open System
    open System.Collections.Generic
    open FScrobble.Core.AppConfigurations

    let allowedPlayer : AllowedPlayer =
        { Name = ""
          Metadata = Some (Dictionary()) }

    let scrobbling : Scrobbling =
        { PlaybackPollInterval = TimeSpan.FromSeconds(3.0)
          MprisServicesPollInterval = TimeSpan.FromSeconds(30.0)
          AllowedPlayers = [||]
          ThresholdFixed = TimeSpan.FromMinutes(4.0)
          ThresholdFraction = 0.5
          ThresholdMin = TimeSpan.FromSeconds(90.0) }

    let logging : Logging =
        { LogLevel = { Default = "Information"; ``Microsoft.Hosting.Lifetime`` = "Information" } }

    let lastFm : LastFm =
        { ApiKey = ""; ApiSecret = ""; SessionKey = ""; BaseUrl = "https://ws.audioscrobbler.com/2.0/"; IsEnabled = false }

    let libreFm : LibreFm =
        { ApiKey = ""; BaseUrl = "https://libre.fm/"; IsEnabled = false }

    let appConfigurations : AppConfigurations =
        { Logging = logging
          Scrobbling = scrobbling
          LastFm = lastFm
          LibreFm = libreFm }
