namespace MicroFun.Generators

open System
open System.IO
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Threading
open System.Threading.Tasks


type InputContentSchemeSelectorLoader(loaders: Map<string, IInputContentUriLoader>, ?fallback: IInputContentUriLoader) =
    let loadContent cancel (uri: Uri) =
        task {
            match loaders |> Map.tryFind uri.Scheme, fallback with
            | Some loader, _
            | None, Some loader ->
                return! loader.LoadContent(uri, cancel)
            | None, None ->
                return failwithf $"Unrecognized uri scheme: {uri.Scheme}"
        }

    interface IInputContentUriLoader with
        member this.LoadContent(uri, cancel) = loadContent cancel uri


type InputContentRespecLoader(loader: IInputContentUriLoader, ?contentType: string, ?encoding: Encoding, ?baseUri: Uri) =
    let loadContent cancel uri =
        task {
            let! inner = loader.LoadContent(uri, cancel)

            let inputContent =
                InputContent.builder
                |> InputContent.withBaseUri (baseUri |> Option.defaultValue inner.BaseUri)
                |> InputContent.withEncoding (encoding |> Option.defaultValue inner.Encoding)
                |> InputContent.withContentType (
                    contentType
                    |> Option.defaultValue inner.ContentType
                )
                |> InputContent.withStream inner.Stream
                |> InputContent.withDispose inner
                |> InputContent.build

            return inputContent
        }

    interface IInputContentUriLoader with
        member this.LoadContent(uri, cancel) = loadContent cancel uri


type InputContentStreamSourceLoader
    (
        loader: CancellationToken -> Uri -> Task<Stream>,
        baseUri: Uri,
        contentType: string,
        encoding: Encoding
    ) =
    let loadContent cancel uri =
        task {
            let! stream = loader cancel uri

            let inputContent =
                InputContent.builder
                |> InputContent.withBaseUri baseUri
                |> InputContent.withEncoding encoding
                |> InputContent.withContentType contentType
                |> InputContent.withStream stream
                |> InputContent.build

            return inputContent
        }

    interface IInputContentUriLoader with
        member this.LoadContent(uri, cancel) = loadContent cancel uri


type InputContentFileLoader() =
    let loadContent cancel (uri: Uri) =
        task {
            let filePath = uri.LocalPath
            let fileInfo = FileInfo(filePath)
            let stream = fileInfo.OpenRead()

            let inputContent =
                InputContent.builder
                |> InputContent.withBaseUri uri
                |> InputContent.withStream stream
                |> InputContent.build

            return inputContent
        }

    static member val Default = InputContentFileLoader()

    interface IInputContentUriLoader with
        member this.LoadContent(uri, cancel) = loadContent cancel uri


type InputContentHttpLoader(httpClient: HttpClient, ?configureRequest: HttpRequestMessage -> HttpRequestMessage) =
    let decodeMediaTypeHeader (ct: MediaTypeHeaderValue) =
        ct
        |> Option.ofObj
        |> Option.map (fun ct ->
            let contentType =
                ct.MediaType
                |> Option.ofObj
                |> Option.defaultValue InputContent.DefaultContentType

            let encoding =
                ct.CharSet
                |> Option.ofObj
                |> Option.map Encoding.GetEncoding
                |> Option.defaultValue InputContent.DefaultEncoding

            contentType, encoding)
        |> Option.defaultValue (InputContent.DefaultContentType, InputContent.DefaultEncoding)

    let ofHttpContent cancel uri (content: HttpContent) =
        task {
            let contentType, encoding =
                decodeMediaTypeHeader content.Headers.ContentType

            let! stream = content.ReadAsStreamAsync(cancel)

            return
                InputContent.builder
                |> InputContent.withBaseUri uri
                |> InputContent.withEncoding encoding
                |> InputContent.withContentType contentType
                |> InputContent.withStream stream
                |> InputContent.build
        }

    let loadContent (cancel: CancellationToken) (uri: Uri) =
        task {
            let request =
                new HttpRequestMessage(HttpMethod.Get, uri)
                |> (configureRequest |> Option.defaultValue id)

            let! response = httpClient.SendAsync(request, cancel)

            response.EnsureSuccessStatusCode() |> ignore

            match response.Content with
            | null ->
                response.Dispose()
                request.Dispose()
                return failwith "Response has no content"

            | content -> return! ofHttpContent cancel uri content
        }

    interface IInputContentUriLoader with
        member this.LoadContent(uri, cancel) = loadContent cancel uri
