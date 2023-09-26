[<AutoOpen>]
module MicroFun.Generators.Preamble

open System
open System.Text
open System.Threading
open System.Threading.Tasks


let defer dispose =
    { new IDisposable with member this.Dispose() = dispose() }

let dispose (disposable: #IDisposable) =
    match disposable with
    | null -> ()
    | d -> d.Dispose()

let disposeAny (disposable: obj) =
    match disposable with
    | :? IDisposable as d -> d.Dispose()
    | _ -> ()


[<RequireQualifiedAccess>]
module Seq =
    let duplicates (xs: seq<'a>) =
        xs
        |> Seq.groupBy id
        |> Seq.filter (fun (_, ys) -> Seq.length ys > 1)
        |> Seq.map fst


[<RequireQualifiedAccess>]
module Task =

    let bind (f: 'a -> Task<'b>) (ma: Task<'a>) =
        task {
            let! a = ma
            return! f a
        }

    let map (f: 'a -> 'b) =
        bind (fun a -> task { return f a })

    let ignore ma = map ignore ma

    let whenAll (tasks: seq<'a Task>) = Task.WhenAll(tasks)

    let whenAllVoid (tasks: seq<Task>) = Task.WhenAll(tasks)

    let whenAny (tasks: seq<'a Task>) = Task.WhenAny(tasks)

    let whenAnyVoid (tasks: seq<Task>) = Task.WhenAny(tasks)


[<RequireQualifiedAccess>]
module Stream =
    open System.IO

    let withPosition position (stream: Stream) = 
        stream.Position <- position
        stream

    [<RequireQualifiedAccess>]
    module Async =

        [<RequireQualifiedAccess>]
        module WithEncoding =
            let withTextWriter (encoding: Encoding) (fn: TextWriter -> Task) (stream: Stream) =
                task {
                    use writer : TextWriter = new StreamWriter(stream, encoding, leaveOpen = true)
                    do! fn writer
                    do! writer.FlushAsync()
                    return stream
                }

        let withTextWriter (fn: TextWriter -> Task) (stream: Stream) =
            task {
                use writer : TextWriter = new StreamWriter(stream, leaveOpen = true)
                do! fn writer
                do! writer.FlushAsync()
                return stream
            }

    [<RequireQualifiedAccess>]
    module WithEncoding =
        let withTextWriter (encoding: Encoding) (fn: TextWriter -> unit) (stream: Stream) =
            use writer : TextWriter = new StreamWriter(stream, encoding, leaveOpen = true)
            fn writer
            writer.Flush()
            stream

    let withTextWriter (fn: TextWriter -> unit) (stream: Stream) =
        use writer : TextWriter = new StreamWriter(stream, leaveOpen = true)
        fn writer
        writer.Flush()
        stream
