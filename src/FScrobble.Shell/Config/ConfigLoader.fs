namespace FScrobble.Shell

open Microsoft.Extensions.Configuration
open System

module ConfigLoader =
    open FScrobble.Core.AppConfigurations
    open FScrobble.Core
    open System.IO
    open Microsoft.Extensions.Hosting
    open Microsoft.Extensions.Configuration.Json


    let inline tryGet<'T when 'T: not struct> (cfg: IConfiguration) (sectionName: string) (fallback: 'T) : 'T =
        match cfg.GetSection(sectionName).Get<'T>() with
        | null -> fallback
        | value -> value


    let configPath =
        match Environment.GetEnvironmentVariable Constants.XDG_CONFIG_HOME with
        | null
        | "" ->
            Path.Combine(
                System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "fscrobble"
            )
        | xdg -> Path.Combine(xdg, "fscrobble")

    
    let getUserConfigSource configPath = 
        JsonConfigurationSource(
                    Path = "config.json",
                    Optional = true,
                    ReloadOnChange = true,
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(configPath)
                )

    let insertUserConfigProvider (builder:HostApplicationBuilder) (source:IConfigurationSource)=
        let env = builder.Environment
        let sources = builder.Configuration.Sources

        let tryFindConfigIndex path =
            sources
            |> Seq.tryFindIndex (function
                | :? JsonConfigurationSource as j when j.Path.Equals(path, StringComparison.OrdinalIgnoreCase) -> true
                | _ -> false)
            |> Option.map ((+) 1)
            
        let indexAfterBaseJson =
            [ $"appsettings.{env.EnvironmentName}.json"; $"appsettings.json" ]
            |> List.tryPick tryFindConfigIndex
            |> Option.defaultValue sources.Count    
                   
        sources.Insert(indexAfterBaseJson, source)
        

 
    let createUserConfigIfNotExists (appBaseDir) =
        let appSettingsPath = Path.Combine(appBaseDir, "appsettings.json")
        let configDir = configPath

        if not (Directory.Exists configDir) then
            Directory.CreateDirectory configDir |> ignore

        let userConfigPath = Path.Combine(configDir, "config.json")

        if not (File.Exists userConfigPath) then
            if File.Exists appSettingsPath then
                File.Copy(appSettingsPath, userConfigPath)
                printfn $"Copied appsettings.json to user config at {userConfigPath}"
            else
                printfn $"Warning: appsettings.json not found at {appSettingsPath}, cannot create default config."
        else
            printfn $"User config already exists at {userConfigPath}"

    let load (cfg: IConfiguration) : AppConfigurations =
        { Logging = tryGet cfg "Logging" Configurations.Defaults.logging
          Scrobbling = tryGet cfg "Scrobbling" Configurations.Defaults.scrobbling
          LastFm = tryGet cfg "LastFm" Configurations.Defaults.lastFm
          LibreFm = tryGet cfg "LibreFm" Configurations.Defaults.libreFm }
        
