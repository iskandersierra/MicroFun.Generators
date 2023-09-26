namespace MicroFun.Generators

open System
open System.IO
open System.Text


type internal TextTemplate(text: string, baseUri: Uri) =
    let relativeUri = Uri("output.txt", UriKind.Relative)
    let contentType = "text/plain"
    let encoding = Encoding.UTF8

    interface ITemplate with
        member _.BaseUri = baseUri

        member _.RenderAsync(context, cancel) =
            task {
                let output =
                    OutputContent.builder
                    |> OutputContent.withRelativeUri relativeUri
                    |> OutputContent.withContentType contentType
                    |> OutputContent.withEncoding encoding
                    |> OutputContent.withWriter (fun cancel stream ->
                        task {
                            use writer = new StreamWriter(stream, encoding, leaveOpen = true)
                            do! writer.WriteAsync(text.AsMemory(), cancel)
                        })
                    |> OutputContent.build

                return output
            }

        member _.Dispose() = ()


type TextTemplateFactory() =
    static member val Default: ITemplateFactory = TextTemplateFactory()

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                use reader =
                    new StreamReader(content.Stream, content.Encoding, leaveOpen = true)

                let! text = reader.ReadToEndAsync(cancel)
                return new TextTemplate(text, content.BaseUri) :> ITemplate
            }
