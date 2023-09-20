namespace MicroFun.Generators

open System
open System.IO
open System.Threading
open System.Threading.Tasks


type internal TextTemplate(reader: TextReader, baseUri: Uri) =
    let mutable text = ""
    let mutable isLoaded = false
    let loadLock = new SemaphoreSlim(1, 1)

    let getText() =
        task {
            if isLoaded then
                return text
            else
                do! loadLock.WaitAsync()
                use _ = defer (fun() -> loadLock.Release() |> ignore)

                let! buffer = reader.ReadToEndAsync()

                text <- buffer

                return text
        }

    interface ITemplate with
        member this.BaseUri = baseUri

        member this.RenderAsync(context, cancel): Task = 
            task {
                let! text = getText()
                do! context.writer.WriteAsync(text)
            }

    interface IDisposable with
        member this.Dispose() =
            reader.Dispose()
            loadLock.Dispose()

type TextTemplateFactory() =
    static member val Default: ITemplateFactory = TextTemplateFactory()

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                return new TextTemplate(content.reader, content.baseUri)
            }
