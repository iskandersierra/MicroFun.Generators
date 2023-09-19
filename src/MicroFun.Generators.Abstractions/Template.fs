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


type TemplateContent =
    { baseUri: Uri
      contentType: string
      content: string }

[<RequireQualifiedAccess>]
module TemplateContent =
    [<Literal>]
    let DefaultContentType = "text/plain"

    [<Literal>]
    let DefaultBaseUri = "urn:"

    let empty = {
        TemplateContent.baseUri = Uri(DefaultBaseUri)
        contentType = DefaultContentType
        content = String.Empty }

    let inline create baseUri contentType content =
        { baseUri = Uri(baseUri)
          contentType = contentType
          content = content }

    let inline withBaseUri baseUri this = { this with baseUri = baseUri }

    let inline withContentType contentType this = { this with contentType = contentType }

    let inline withContent content this = { this with content = content }

    let inline ofContent content = empty |> withContent content


type ITemplateFactory =
    abstract member CreateTemplateAsync: content: TemplateContent * cancel: CancellationToken -> Task<ITemplate>

[<RequireQualifiedAccess>]
module TemplateFactory =
    [<RequireQualifiedAccess>]
    module WithCancel =
        let inline createTemplate cancel (content: TemplateContent) (template: ITemplateFactory) =
            template.CreateTemplateAsync(content, cancel)


    let inline createTemplate (content: TemplateContent) (template: ITemplateFactory) =
        template.CreateTemplateAsync(content, CancellationToken.None)
