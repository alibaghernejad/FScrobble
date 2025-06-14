namespace FScrobble.Shell

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open FScrobble.Shell.CommandLineInterface

module Program =
    open Microsoft.Extensions.Logging
    open FScrobble.Core
    open System.Threading

    [<EntryPoint>]
    let main args =

        let builder = Host.CreateApplicationBuilder(args)
        builder.Services.AddHostedService<ScrobblingDeamon>() |> ignore
        let host = builder.Build()
        let ctSource = new CancellationTokenSource()
        let ct = ctSource.Token
        
        let logger = host.Services.GetService<ILogger>()
        let deps = CompositionRoot.buildAppDependencies (builder.Configuration, logger, ct)

        match List.ofArray (args) with
        | [] -> host.Run()
        | _ ->
            let program = runCommandLineInterface args
            ReaderAsync.run program deps |> Async.RunSynchronously |> ignore
        0 // exit code
