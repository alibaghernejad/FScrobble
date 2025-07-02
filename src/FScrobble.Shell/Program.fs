namespace FScrobble.Shell

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FScrobble.Shell.CommandLineInterface

module Program =
    open Microsoft.Extensions.Logging
    open FScrobble.Core
    open System.Threading
    open System.IO
    open System
    open Microsoft.Extensions.Configuration
    open FScrobble.Core.Dependencies

    [<EntryPoint>]
    let main args =

        let contentRoot = AppDomain.CurrentDomain.BaseDirectory
        printfn "Current directory set to: %s" (Directory.GetCurrentDirectory())
        try
           ConfigLoader.createUserConfigIfNotExists contentRoot
        with ex ->
            printfn $"Warning: Could not create default config: {ex.Message}"
                
        let ctSource = new CancellationTokenSource()
        let ct = ctSource.Token
        
        let builder =
            Host.CreateApplicationBuilder(
                HostApplicationBuilderSettings(
                    ContentRootPath = contentRoot,
                    Args = args
                )
            )        
        
        let userConfigPath = ConfigLoader.configPath
        let userConfigSource = ConfigLoader.getUserConfigSource userConfigPath
        ConfigLoader.insertUserConfigProvider builder userConfigSource

        builder.Services.AddHostedService<ScrobblingDaemon>() |> ignore
        builder.Services.AddSingleton<AppDependencies>(fun a -> 
                            let loggerFactory = a.GetService<ILoggerFactory>()
                            let logger = loggerFactory.CreateLogger "FScrobble.Shell"
                            CompositionRoot.buildAppDependencies (a.GetRequiredService<IConfiguration>(), logger, ct))
                          |> ignore

            
        let host = builder.Build()
        let deps = host.Services.GetRequiredService<AppDependencies>()

        match List.ofArray (args) with
        | [] -> host.Run()
        | _ ->
            let program = runCommandLineInterface args
            ReaderAsync.run program deps |> Async.RunSynchronously |> ignore
        0 // exit code
