[<AutoOpen>]
module MicroFun.Generators.Commands.Preamble

open System
open System.IO
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open FsToolkit.ErrorHandling

open MicroFun.Generators

let getHttpReader (cancel: CancellationToken) (httpClient: HttpClient) (url: Uri) =
    task {
        let! response = httpClient.GetAsync(url, cancel)
        match response.Content with
        | null ->
            response.Dispose()
            httpClient.Dispose()
            return failwith "Response has no content"
        | content ->
            let encoding =
                response.Content.Headers.ContentType
                |> Option.ofObj
                |> Option.bind (fun ct -> Option.ofObj ct.CharSet)
                |> Option.defaultValue "utf-8"
                |> Encoding.GetEncoding

            let! stream = content.ReadAsStreamAsync(cancel)

            let reader = new StreamReader(stream, encoding, leaveOpen = false)

            let dispose = defer (fun() ->
                reader.Dispose()
                stream.Dispose()
                response.Dispose()
            )

            return reader, dispose
    }

let getFileReader (cancel: CancellationToken) (filePath: string) =
    task {
        let fileInfo = FileInfo(filePath)
        let stream = fileInfo.OpenRead()
        let reader = new StreamReader(stream, Encoding.UTF8, leaveOpen = false)
        let dispose = defer (fun() ->
            reader.Dispose()
            stream.Dispose()
        )
        return reader, dispose
    }

let getSourceReader (cancel: CancellationToken) (httpClient: HttpClient Lazy) (source: string) =
    task {
        match Uri.TryCreate(source, UriKind.Absolute) with
        | true, url ->
            match url.Scheme with
            | "http" | "https" ->
                return! getHttpReader cancel httpClient.Value url
            | "file" ->
                return! getFileReader cancel url.LocalPath
            | scheme ->
                return failwithf "Unsupported scheme: %s" scheme
        | false, _ ->
            return! getFileReader cancel source
    }

type ModelFormatEnum =
    | Json = 0

let readModel (cancel: CancellationToken) (http: Lazy<HttpClient>) (source: string) (format: ModelFormatEnum) (type': string) =
    task {
        return obj()
    }
