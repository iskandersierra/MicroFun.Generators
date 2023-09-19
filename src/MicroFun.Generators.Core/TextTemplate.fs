namespace MicroFun.Generators

open System
open System.Threading.Tasks


type internal TextTemplate(content: string, baseUri: Uri) =
    interface ITemplate with
        member this.BaseUri = baseUri

        member this.RenderAsync(context, cancel): Task = 
            task {
                do! context.writer.WriteAsync(content)
            }

type TextTemplateFactory() =
    static member val Default: ITemplateFactory = TextTemplateFactory()

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                return TextTemplate(content.content, content.baseUri)
            }
