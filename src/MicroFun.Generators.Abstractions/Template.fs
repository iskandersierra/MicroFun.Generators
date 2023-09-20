namespace MicroFun.Generators

open System
open System.IO
open System.Threading
open System.Threading.Tasks

type RenderTemplateContext = {
    model: obj
    writer: TextWriter
}

type ITemplate =
    abstract BaseUri: Uri with get
    abstract RenderAsync: context: RenderTemplateContext * cancel: CancellationToken -> Task

[<AutoOpen>]
module TemplateExtensions =
    type ITemplate with
        member this.RenderAsync(model: obj, cancel: CancellationToken) =
            task {
                use writer = new StringWriter()
                let context = { model = model; writer = writer }
                do! this.RenderAsync(context, cancel)
                return writer.ToString()
            }

[<RequireQualifiedAccess>]
module Template =
    [<RequireQualifiedAccess>]
    module WithCancel =
        let inline renderContext cancel (context: RenderTemplateContext) (template: ITemplate) =
            template.RenderAsync(context, cancel)

        let inline renderModel cancel (model: obj) (template: ITemplate) =
            template.RenderAsync(model, cancel)


    let inline renderContext (context: RenderTemplateContext) (template: ITemplate) =
        template.RenderAsync(context, CancellationToken.None)

    let inline renderModel (model: obj) (template: ITemplate) =
        template.RenderAsync(model, CancellationToken.None)
