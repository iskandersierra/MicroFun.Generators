namespace MicroFun.Generators

open System
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks

open FsToolkit.ErrorHandling


type StreamSink = CancellationToken -> Stream -> Task


type IOutputContent =
    abstract RelativeUri : Uri
    abstract ContentType : string
    abstract Encoding : Encoding
    abstract Write : Stream * CancellationToken -> Task


type OutputContentBuilder =
    { relativeUri: Uri option
      contentType: string option
      encoding: Encoding option
      writer: StreamSink option }


[<RequireQualifiedAccess>]
module OutputContent =
    [<Literal>]
    let DefaultContentType = "text/plain"

    let DefaultEncoding = Encoding.UTF8

    let builder: OutputContentBuilder =
        { relativeUri = None
          contentType = None
          encoding = None
          writer = None }

    let withRelativeUri relativeUri (this: OutputContentBuilder) =
        { this with relativeUri = Some relativeUri }

    let withContentType contentType (this: OutputContentBuilder) =
        { this with contentType = Some contentType }

    let withEncoding encoding (this: OutputContentBuilder) =
        { this with encoding = Some encoding }

    let withWriter writer (this: OutputContentBuilder) = { this with writer = Some writer }

    let tryBuild (this: OutputContentBuilder) =
        result {
            let! relativeUri =
                match this.relativeUri with
                | Some relativeUri -> Ok relativeUri
                | None -> Error "Cannot build input content without relative URI"

            let contentType =
                this.contentType
                |> Option.defaultValue DefaultContentType

            let encoding =
                this.encoding
                |> Option.defaultValue DefaultEncoding

            let! write =
                match this.writer with
                | Some write -> Ok write
                | None -> Error "Cannot build input content without writer"

            return
                { new IOutputContent with
                    member _.RelativeUri = relativeUri
                    member _.ContentType = contentType
                    member _.Encoding = encoding
                    member _.Write(stream, cancel) = write cancel stream }
        }

    let build (this: OutputContentBuilder) =
        match tryBuild this with
        | Ok content -> content
        | Error error -> failwith error

    let toText (this: IOutputContent) : Task<string> =
        task {
            use stream = new MemoryStream()
            do! this.Write(stream, CancellationToken.None)
            stream.Seek(0L, SeekOrigin.Begin) |> ignore
            use reader = new StreamReader(stream, this.Encoding, leaveOpen = true)
            return! reader.ReadToEndAsync()
        }


type OutputContentBuilder with
    static member Create() = OutputContent.builder

    member this.WithRelativeUri(relativeUri) =
        this |> OutputContent.withRelativeUri relativeUri

    member this.WithContentType(contentType) =
        this |> OutputContent.withContentType contentType

    member this.WithEncoding(encoding) =
        this |> OutputContent.withEncoding encoding

    member this.WithWrite(writer) = this |> OutputContent.withWriter writer

    member this.TryBuild() = this |> OutputContent.tryBuild
    member this.Build() = this |> OutputContent.build
