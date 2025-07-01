namespace FScrobble.Core

module ReaderAsync =
    open System.Threading.Tasks
    type ReaderAsync<'env, 'a> = ReaderAsync of ('env -> Async<'a>)

    let run (ReaderAsync readerFn) env = readerFn env

    let returnM x = ReaderAsync (fun _ -> async.Return x)

    let bind f (ReaderAsync r) =
        ReaderAsync (fun env -> async {
            let! a = r env
            let (ReaderAsync r') = f a
            return! r' env
        })

    let map f (ReaderAsync r) =
        ReaderAsync (fun env -> async {
            let! a = r env
            return f a
        })

    let tryWith (ReaderAsync computation: ReaderAsync<'env, 'a>) (handler: exn -> ReaderAsync<'env, 'a>) : ReaderAsync<'env, 'a> =
        ReaderAsync(fun env ->
            async {
                try
                    return! computation env
                with ex ->
                    let (ReaderAsync r) = handler ex 
                    return! r env
            })

    let tryFinally (ReaderAsync computation: ReaderAsync<'env, 'a>) (compensation: unit -> unit) : ReaderAsync<'env, 'a> =
        ReaderAsync(fun env ->
            async {
                try
                    return! computation env
                finally
                    compensation()
            })
    

    type ReaderAsyncBuilder() =
        member _.Return(x) = returnM x
        member _.Bind(r, f) = bind f r
        // Overload: Allow using plain Async<'a> inside the CE
        member _.Bind(m: Async<'a>, f: 'a -> ReaderAsync<'env, 'b>) : ReaderAsync<'env, 'b> =
            ReaderAsync(fun env -> async {
                let! a = m
                let (ReaderAsync r2) = f a
                return! r2 env
            })
            
        member _.Delay(f: unit -> ReaderAsync<'env, 'a>) : ReaderAsync<'env, 'a> =
            ReaderAsync (fun env -> async.Delay(fun () -> let (ReaderAsync r) = f() in r env))     

        member _.TryWith(m, h) = tryWith m h
        member _.TryFinally(m, c) = tryFinally m c
        member _.Using(resource: #System.IDisposable, body: _ -> ReaderAsync<'env, 'a>) : ReaderAsync<'env, 'a> =
            ReaderAsync(fun env ->
                async.Using(resource, fun r ->
                    let (ReaderAsync inner) = body r
                    inner env
                ))

        member _.Combine(a, b) = bind (fun () -> b) a    
        member _.ReturnFrom(r) = r
        member _.Zero() = ReaderAsync (fun _ -> async.Zero())

    let readerAsync = ReaderAsyncBuilder()

    let ask = ReaderAsync (fun env -> async.Return env)

    let liftAsync (a: Async<'a>) : ReaderAsync<'env, 'a> =
        ReaderAsync(fun f -> a)

    let liftTask (t: Task<'a>) : ReaderAsync<'env, 'a> =
        t |> Async.AwaitTask |> liftAsync

    let liftToAsync (reader: 'env -> 'a) : 'env -> Async<'a> =
        fun env -> async { return reader env }

    let inline askFor<'env> : ReaderAsync<'env, 'env> = ask
