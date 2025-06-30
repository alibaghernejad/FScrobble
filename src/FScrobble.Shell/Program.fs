namespace FScrobble.Shell

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FScrobble.Shell.CommandLineInterface
open System.Linq

module Program =
    open Microsoft.Extensions.Logging
    open FScrobble.Core
    open System.Threading
    open System.Diagnostics
    open System.IO
    open System
    open Microsoft.Extensions.Configuration

    [<EntryPoint>]
    let main args =

        let contentRoot = AppDomain.CurrentDomain.BaseDirectory
        printfn "Current directory set to: %s" (Directory.GetCurrentDirectory())
        
        let builder = Host.CreateDefaultBuilder(args)
        let host = 
            builder
                .UseContentRoot(contentRoot) 
                .ConfigureServices(fun ctx services ->
                    services.AddHostedService<ScrobblingDaemon>() |> ignore
                )
                .Build()
                

        let ctSource = new CancellationTokenSource()
        let ct = ctSource.Token

        let loggerFactory = host.Services.GetService<ILoggerFactory>()
        let logger = loggerFactory.CreateLogger("FScrobble.Shell")
        let deps = CompositionRoot.buildAppDependencies (host.Services.GetRequiredService<IConfiguration>(), logger, ct)

        match List.ofArray (args) with
        | [] -> host.Run()
        | _ ->
            let program = runCommandLineInterface args
            ReaderAsync.run program deps |> Async.RunSynchronously |> ignore
        0 // exit code
