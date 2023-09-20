namespace MicroFun.Generators

open System.Threading
open System.Threading.Tasks


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
