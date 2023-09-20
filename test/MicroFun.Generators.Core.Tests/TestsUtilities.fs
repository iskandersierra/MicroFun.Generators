[<AutoOpen>]
module MicroFun.Generators.Core.Tests.TestsUtilities

open System
open System.IO
open System.Threading.Tasks

open MicroFun.Generators


type MockTextTemplate(reader: TextReader, baseUri: Uri) =
    let content = reader.ReadToEnd()

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
                return MockTextTemplate(content.reader, content.baseUri)
            }
