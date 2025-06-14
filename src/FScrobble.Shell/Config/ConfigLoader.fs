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

    let load2 (cfg: IConfiguration) : AppConfigurations =
        { Logging = tryGet cfg "Logging" Configurations.Defaults.logging
          Scrobbling = tryGet cfg "Scrobbling" Configurations.Defaults.scrobbling
          LastFm = tryGet cfg "LastFm" Configurations.Defaults.lastFm
          LibreFm = tryGet cfg "LibreFm" Configurations.Defaults.libreFm }

    let load (cfg: IConfiguration) : AppConfigurations =
        // let defaultScrobblingConfig = {PollForScrobbleMs =2000; AllowedPlayers=[||]  }
        
        // let section = cfg.GetSection("Scrobbling:AllowedPlayers").Get<AllowedPlayer>()
        let res = 
          { Logging = cfg.GetSection("Logging").Get<Logging>()
            Scrobbling = cfg.GetSection("Scrobbling").Get<Scrobbling>()

            LastFm = cfg.GetSection("LastFM").Get<LastFm>()
            LibreFm = cfg.GetSection("LastFM").Get<LibreFm>()
            // MusicBrainz = cfg.GetSection("LastFM").Get<MusicBrainz>()
            
          }
        res
