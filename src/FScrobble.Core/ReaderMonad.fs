namespace FScrobble.Core

module Reader =

    type Reader<'env, 'a> = Reader of ('env -> 'a)

    let run (Reader r) env = r env

    let returnM x = Reader(fun _ -> x)

    let map f (Reader r) = Reader(fun env -> f (r env))

    let bind f (Reader r) =
        Reader(fun env ->
            let a = r env
            let (Reader r') = f a
            r' env)
    let ask : Reader<'env, 'env> = Reader id
    type ReaderBuilder() =
        member _.Return(x) = returnM x
        member _.Bind(r, f) = bind f r
        member _.ReturnFrom(r) = r
        member _.Zero() = Reader (fun _ -> ())
    let reader = ReaderBuilder()


