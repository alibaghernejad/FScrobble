namespace FScrobble.Shell

open Microsoft.Extensions.Configuration
open System
module ConfigLoader =
    open FScrobble.Core.AppConfigurations
    open FScrobble.Core
    open FScrobble.Core
    

    let inline tryGet<'T when 'T : not struct> (cfg: IConfiguration) (sectionName: string) (fallback: 'T) : 'T= 
      match cfg.GetSection(sectionName).Get<'T>() with
      | null -> fallback
      | value -> value

    let load (cfg: IConfiguration) : AppConfigurations =
        { Logging = tryGet cfg "Logging" Configurations.Defaults.logging
          Scrobbling = tryGet cfg "Scrobbling" Configurations.Defaults.scrobbling
          LastFm = tryGet cfg "LastFm" Configurations.Defaults.lastFm
          LibreFm = tryGet cfg "LibreFm" Configurations.Defaults.libreFm }
