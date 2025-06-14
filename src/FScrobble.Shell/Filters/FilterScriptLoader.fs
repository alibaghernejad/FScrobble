namespace FScrobble.Filters
// module FilterScriptLoader =

    // open System
    // open System.IO
    // open System.Reflection
    // open FSharp.Compiler.CodeAnalysis
    // open FSharp.Compiler.Text

    // let loadTryFilters<'T> (scriptFolder: string) : ('T -> option<'T>) list =
    //     let checker = FSharpChecker.Create()
    //     let tempPath = Path.Combine(Path.GetTempPath(), "fsfilters")
    //     Directory.CreateDirectory(tempPath) |> ignore

    //     Directory.GetFiles(scriptFolder, "*.fs")
    //     |> Array.collect (fun scriptPath ->
    //         try
    //             let scriptText = File.ReadAllText(scriptPath)
    //             let fileName = Path.GetFileNameWithoutExtension(scriptPath)
    //             let sourceText = SourceText.ofString scriptText

    //             // Get project options
    //             let projOptions, _ =
    //                 checker.GetProjectOptionsFromScript(scriptPath, sourceText)
    //                 |> Async.RunSynchronously

    //             // let projOptions =
    //             //     { projOptions with
    //             //         ProjectFileName = scriptPath + ".fsproj"  // Add .fsproj (even if it's fake)
    //             //         SourceFiles = [| scriptPath |]            // Important: this must point to actual .fs file
    //                 // }
    //             // We'll compile to DLL file in temp folder
    //             let dllPath = Path.Combine(tempPath, $"{fileName}.dll")

    //             // Modify options to output DLL at dllPath
    //             // projOptions.OtherOptions contains compiler flags; adjust -o flag accordingly

    //             let sourceFiles = [| scriptPath |]  // your .fs file path(s)

    //             let otherOptions =
    //                 projOptions.OtherOptions
    //                 |> Array.filter (fun opt -> not (opt.StartsWith("-o:")))
    //                 // |> Array.append [| "-o:" + dllPath; "-a" |] // -a for library output
    //                 |> Array.append (Array.append sourceFiles [| "-o:" + dllPath; "-a" |])
    //                 |> Array.append [|"test"|]


    //             let updatedProjOptions =
    //                 { projOptions with OtherOptions = otherOptions }

    //             // Compile using checker.Compile
    //             // When using checker.Compile with command-line style options (OtherOptions), you do need to include the source files explicitly i
    //             // The First Argument will be ignored, so we can use a dummy value like "test" or an empty string.
    //             let errors, exitCode =
    //                 checker.Compile(updatedProjOptions.OtherOptions)
    //                 |> Async.RunSynchronously

    //             if errors.Length > 0 then
    //                 errors
    //                 |> Array.iter (fun e ->
    //                     Console.Error.WriteLine($"[Filter Load Error] {fileName}: {e.Message}")
    //                 )

    //             if exitCode.IsSome && File.Exists(dllPath) then
    //                 let asm = Assembly.LoadFile(dllPath)
    //                 asm.GetTypes()
    //                 |> Array.collect (fun t ->
    //                     t.GetMethods(BindingFlags.Public ||| BindingFlags.Static)
    //                     |> Array.choose (fun m ->
    //                         if m.Name = "tryFilter" && m.GetParameters().Length = 1 then
    //                             try
    //                                 let func = fun (x: 'T) ->
    //                                     let res = m.Invoke(null, [| box x |])
    //                                     res :?> option<'T>
    //                                 Some func
    //                             with ex ->
    //                                 Console.Error.WriteLine($"[Delegate Error] {fileName}: {ex.Message}")
    //                                 None
    //                         else None))
    //             else
    //                 Console.Error.WriteLine($"[Compile Error] {fileName}: Compilation failed with exit code {exitCode}")
    //                 [||]
    //         with ex ->
    //             Console.Error.WriteLine($"[Script Load Error] {scriptPath}: {ex.Message}")
    //             [||])
    //     |> Array.toList
    //     // |> List.concat