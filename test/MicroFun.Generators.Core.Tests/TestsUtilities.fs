[<AutoOpen>]
module MicroFun.Generators.Core.Tests.TestsUtilities

open System
open System.Threading.Tasks

open MicroFun.Generators


type MockTextTemplate(content: string, baseUri: Uri) =
    interface ITemplate with
        member this.BaseUri = baseUri

        member this.RenderAsync(context, _): Task = 
            task {
                do! context.writer.WriteAsync(content)
            }

type MockTextTemplateFactory(content: TemplateContent) =
    interface ITemplateFactory with
        member this.CreateTemplateAsync(_, _) =
            task {
                return MockTextTemplate(content.content, content.baseUri)
            }
