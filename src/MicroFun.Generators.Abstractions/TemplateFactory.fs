namespace MicroFun.Generators

open System.Threading
open System.Threading.Tasks


type ITemplateFactory =
    abstract member CreateTemplateAsync: content: IInputContent * cancel: CancellationToken -> Task<ITemplate>


[<RequireQualifiedAccess>]
module TemplateFactory =
    [<RequireQualifiedAccess>]
    module WithCancel =
        let createTemplate cancel (content: IInputContent) (template: ITemplateFactory) =
            template.CreateTemplateAsync(content, cancel)


    let createTemplate (content: IInputContent) =
        WithCancel.createTemplate CancellationToken.None content


type ITemplateFactorySelector =
    abstract SelectFactory : contentType: string -> ITemplateFactory


type SelectorTemplateFactory(selector: ITemplateFactorySelector) =
    interface ITemplateFactory with
        member _.CreateTemplateAsync(content, cancel) =
            task {
                let factory =
                    selector.SelectFactory(content.ContentType)

                return! factory.CreateTemplateAsync(content, cancel)
            }
