namespace MicroFun.Generators

open System
open System.Threading
open System.Threading.Tasks


type TemplateRenderContext = { model: obj }


[<RequireQualifiedAccess>]
module TemplateRenderContext =
    let create model = { model = model }


type ITemplate =
    inherit IDisposable
    abstract BaseUri : Uri
    abstract RenderAsync : context: TemplateRenderContext * cancel: CancellationToken -> Task<IOutputContent>


[<RequireQualifiedAccess>]
module Template =
    [<RequireQualifiedAccess>]
    module WithCancel =
        let renderContext cancel (context: TemplateRenderContext) (template: ITemplate) =
            template.RenderAsync(context, cancel)

        let renderModel cancel (model: obj) =
            model
            |> TemplateRenderContext.create
            |> renderContext cancel


    let renderContext (context: TemplateRenderContext) =
        WithCancel.renderContext CancellationToken.None context

    let renderModel (model: obj) =
        model
        |> TemplateRenderContext.create
        |> renderContext
