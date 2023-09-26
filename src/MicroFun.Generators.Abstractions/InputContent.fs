namespace MicroFun.Generators

open System
open System.IO
open System.Text

open FsToolkit.ErrorHandling


type IInputContent =
    inherit IDisposable
    abstract BaseUri : Uri
    abstract ContentType : string
    abstract Encoding : Encoding
    abstract Stream : Stream


type InputContentBuilder =
    { baseUri: Uri option
      contentType: string option
      encoding: Encoding option
      stream: Stream option
      text: string option
      dispose: IDisposable option }


[<RequireQualifiedAccess>]
module InputContent =
    [<Literal>]
    let DefaultContentType = "text/plain"

    let DefaultBaseUri = Uri("urn:")

    let DefaultEncoding = Encoding.UTF8

    let builder: InputContentBuilder =
        { baseUri = None
          contentType = None
          encoding = None
          stream = None
          text = None
          dispose = None }

    let withBaseUri baseUri (this: InputContentBuilder) = { this with baseUri = Some baseUri }

    let withContentType contentType (this: InputContentBuilder) =
        { this with contentType = Some contentType }

    let withEncoding encoding (this: InputContentBuilder) = { this with encoding = Some encoding }

    let withStream stream (this: InputContentBuilder) = { this with stream = Some stream }

    let withText text (this: InputContentBuilder) = { this with text = Some text }

    let withDispose dispose (this: InputContentBuilder) = { this with dispose = Some dispose }

    let tryBuild (this: InputContentBuilder) =
        result {
            let baseUri =
                this.baseUri |> Option.defaultValue DefaultBaseUri

            let contentType =
                this.contentType
                |> Option.defaultValue DefaultContentType

            let encoding =
                this.encoding
                |> Option.defaultValue DefaultEncoding

            let! stream =
                match this.stream, this.text with
                | Some _, Some _ -> Error "Cannot build input content from both stream and text"
                | None, None -> Error "Cannot build input content from neither stream nor text"
                | Some stream, None -> Ok stream
                | None, Some text -> Ok(new MemoryStream(encoding.GetBytes(text)))

            let disposable =
                defer (fun () ->
                    stream.Dispose()

                    match this.dispose with
                    | Some disposable -> disposable.Dispose()
                    | None -> ())

            return
                { new IInputContent with
                    member _.BaseUri = baseUri
                    member _.ContentType = contentType
                    member _.Encoding = encoding
                    member _.Stream = stream
                  interface IDisposable with
                      member _.Dispose() = disposable.Dispose() }
        }

    let build = tryBuild >> Result.either id failwith

    let ofStream stream = builder |> withStream stream |> build

    let ofText text = builder |> withText text |> build


type InputContentBuilder with
    static member Create() = InputContent.builder

    member this.WithBaseUri(baseUri) =
        this |> InputContent.withBaseUri baseUri

    member this.WithContentType(contentType) =
        this |> InputContent.withContentType contentType

    member this.WithEncoding(encoding) =
        this |> InputContent.withEncoding encoding

    member this.WithStream(string) = this |> InputContent.withStream string
    member this.WithText(text) = this |> InputContent.withText text

    member this.WithDispose(dispose) =
        this |> InputContent.withDispose dispose

    member this.TryBuild() = this |> InputContent.tryBuild
    member this.Build() = this |> InputContent.build


//type InputContentSource = CancellationToken -> Task<IInputContent>
