namespace FScrobble.Core
module Async =
    let map f asyncValue =
        async {
            let! v = asyncValue
            return f v
        }
    let bind f x = async {
        let! r = x
        return! f r
    }

    let apply fAsync xAsync =
        async {
            let! f = fAsync
            let! x = xAsync
            return f x
        }