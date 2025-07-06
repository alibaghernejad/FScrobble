namespace FScrobble

open System
open System.Net.Http
open System.Security.Cryptography
open System.Text
open FScrobble.Core.Dependencies

module LastFm =
    open System.IO
    open System.Net
    open FScrobble.Core.ReaderAsync
    open System.Text.Json

    type Track =
        { Artist: string
          Title: string
          Album: string option
          Duration: TimeSpan option
          Timestamp: DateTimeOffset }

    type ApiCredentials =
        { ApiKey: string
          ApiSecret: string
          SessionKey: string }

    type ScrobbleError =
        | NetworkError of string
        | ApiError of int * string
        | AuthenticationError
        | UnexpectedError of exn

    type ScrobbleResult = Result<unit, ScrobbleError>

    let private calculateApiSignature (parameters: (string * string) list) apiSecret =
        let sortedParams =
            parameters
            |> List.sortBy fst
            |> List.map (fun (k, v) -> k + v) // URL-encode values
            |> String.concat ""

        let signatureInput = sortedParams + apiSecret
        use md5 = MD5.Create()

        let hash =
            md5.ComputeHash(Encoding.UTF8.GetBytes(signatureInput))
            |> Array.map (fun b -> b.ToString("x2"))
            |> String.concat ""

        hash

    let private handleHttpResponse (response: HttpResponseMessage) =
        async {
            if response.IsSuccessStatusCode then
                return Ok()
            else
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                return
                    match response.StatusCode with
                    | HttpStatusCode.Unauthorized -> Error AuthenticationError
                    | code -> Error(ApiError(int code, content))
        }

    let getToken (apiKey: string) =
        async {
            try
                use client = new HttpClient()

                let url =
                    sprintf "https://ws.audioscrobbler.com/2.0/?method=auth.getToken&api_key=%s&format=json" apiKey

                let! response = client.GetAsync(url) |> Async.AwaitTask
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                if response.IsSuccessStatusCode then
                    let json = System.Text.Json.JsonDocument.Parse(content)
                    let token = json.RootElement.GetProperty("token").GetString()
                    return Ok token
                else
                    return Error(ApiError(int response.StatusCode, content))
            with
            | :? HttpRequestException as ex -> return Error(NetworkError ex.Message)
            | ex -> return Error(UnexpectedError ex)
        }

    let getAuthenticationUrl (apiKey: string) (token: string) =
        sprintf "https://www.last.fm/api/auth/?api_key=%s&token=%s" apiKey token

    let getSessionKey (apiKey: string) (apiSecret: string) (token: string) =
        async {
            try
                use client = new HttpClient()
                let parameters = [ "method", "auth.getSession"; "api_key", apiKey; "token", token ]

                let signature = calculateApiSignature parameters apiSecret

                let url =
                    parameters
                    |> List.append [ "api_sig", signature; "format", "json" ]
                    |> List.map (fun (k, v) -> sprintf "%s=%s" k (Uri.EscapeDataString(v)))
                    |> String.concat "&"
                    |> sprintf "https://ws.audioscrobbler.com/2.0/?%s"

                let! response = client.GetAsync(url) |> Async.AwaitTask
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                if response.IsSuccessStatusCode then
                    let json = JsonDocument.Parse(content)

                    let sessionKey =
                        json.RootElement.GetProperty("session").GetProperty("key").GetString()

                    return Ok sessionKey
                else
                    return Error(ApiError(int response.StatusCode, content))
            with
            | :? HttpRequestException as ex -> return Error(NetworkError ex.Message)
            | ex -> return Error(UnexpectedError ex)
        }

    let persistSessionKey (sessionKey: string) (filePath: string) =
        async {
            try
                do! File.WriteAllTextAsync(filePath, sessionKey) |> Async.AwaitTask
                return Ok()
            with ex ->
                return Error ex
        }

    let fetchSessionKey (filePath: string) =
        async {
            try
                let! content = File.ReadAllTextAsync(filePath) |> Async.AwaitTask
                return Ok content
            with ex ->
                return Error ex
        }

    let authenticateLastFm (apiKey: string) (apiSecret: string) =
        async {
            // Get Token
            let! tokenResult = getToken apiKey

            match tokenResult with
            | Ok token ->
                printfn "Token obtained: %s" token

                // Generate Authentication URL
                let authUrl = getAuthenticationUrl apiKey token
                printfn "Please authenticate by visiting this URL: %s" authUrl

                // Wait for user confirmation
                printfn "Press Enter after authenticating..."
                Console.ReadLine() |> ignore

                // Get Session Key
                let! sessionKeyResult = getSessionKey apiKey apiSecret token

                match sessionKeyResult with
                | Ok sessionKey ->
                    printfn "Session Key obtained: %s" sessionKey
                    // Persist the session key to a file
                    let filePath = Path.Combine(AppContext.BaseDirectory, ".sessionKey");
                    let! persistResult = persistSessionKey sessionKey filePath

                    match persistResult with
                    | Ok() -> printfn "Session key successfully persisted."
                    | Error err -> printfn "Error persisting session key: %A" err

                    return Ok sessionKey
                | Error err ->
                    printfn "Failed to obtain session key: %A" err
                    return Error err
            | Error err ->
                printfn "Failed to obtain token: %A" err
                return Error err
        }

    let getSessionKeyOrAuthenticate () : ReaderAsync<AppDependencies, Result<string, ScrobbleError>> =
        readerAsync {
            let! deps = ask
            let apiKey = deps.Config.LastFm.ApiKey
            let apiSecret = deps.Config.LastFm.ApiSecret
            let! sessionKey = fetchSessionKey ".sessionKey"

            match sessionKey with
            | Ok sessionKey when not (String.IsNullOrWhiteSpace(sessionKey)) ->
                printfn "Session key fetched from file."
                printfn "Using existing session key."
                return Ok sessionKey
            | Error _
            | _ ->
                printfn "No valid session key found in file. Starting authentication flow..."
                let! result = authenticateLastFm apiKey apiSecret

                match result with
                | Ok newSessionKey ->
                    printfn "New session key obtained."
                    return Ok newSessionKey
                | Error err ->
                    printfn "Authentication failed: %A" err
                    return Error err

        }

    let scrobbleTrackInternal
        (track: Track)
        (credentials: ApiCredentials)
        : ReaderAsync<AppDependencies, Result<unit, ScrobbleError>> =
        readerAsync {
            try
                let! deps = ask
                let apiKey = deps.Config.LastFm.ApiKey
                use client = new HttpClient()
                let timestamp = track.Timestamp.ToUnixTimeSeconds().ToString()

                let parameters =
                    [ "method", "track.scrobble"
                      "artist", track.Artist
                      "track", track.Title
                      "timestamp", timestamp
                      "api_key", credentials.ApiKey
                      "sk", credentials.SessionKey ]

                let parametersWithOptional =
                    match track.Album with
                    | Some album -> ("album", album) :: parameters
                    | None -> parameters

                let signature = calculateApiSignature parametersWithOptional credentials.ApiSecret

                let content =
                    parametersWithOptional
                    |> List.append [ "api_sig", signature ]
                    |> List.map (fun (k, v) -> sprintf "%s=%s" k (Uri.EscapeDataString(v)))
                    |> String.concat "&"

                use content =
                    new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")

                let! response = client.PostAsync(deps.Config.LastFm.BaseUrl, content) |> liftTask
                let! res = handleHttpResponse response
                return res
            with
            | :? HttpRequestException as ex -> return Error(NetworkError ex.Message)
            | ex -> return Error(UnexpectedError ex)
        }

    let scrobbleTrack
        (track: Track)
        (credentials: ApiCredentials) = readerAsync {
            // TODO: let! and!
            let! (deps: AppDependencies) = ask
            let! sessionKeyResult = getSessionKeyOrAuthenticate ()

            match sessionKeyResult with
            | Ok sessionKey ->
                let updatedCredentials =
                    { credentials with
                        SessionKey = sessionKey }

                let! result = scrobbleTrackInternal track updatedCredentials

                match result with
                | Ok() ->
                    deps.Log
                        Core.Logging.LogSeverity.Information
                        $"Successfully scrobbled track: {track.Title} by {track.Artist}"
                        None

                    return result
                | Error error ->
                    deps.Log Core.Logging.LogSeverity.Error $"Failed to scrobble track: {error}" None
                    return Error error
            | Error err ->
                deps.Log
                    Core.Logging.LogSeverity.Warning
                    $"Skipping track: {track.Title} by {track.Artist} due to missing session key. Error: {err}"
                    None

                return Error err
        }
