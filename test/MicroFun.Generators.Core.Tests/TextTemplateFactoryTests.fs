module MicroFun.Generators.Core.Tests.TextTemplateFactoryTests

open System
open Xunit
open Swensen.Unquote

open MicroFun.Generators

[<Fact>]
let ``TextTemplateFactory.Default is not null`` () =
    Assert.NotNull(TextTemplateFactory.Default)

let helloWorld = "Hello world!"

[<Fact>]
let ``TextTemplateFactory creates a TextTemplate`` () =
    task {
        let contentType = "text/plain"
        let baseUri = "https://example.com/"

        let templateContent = TemplateContent.create baseUri contentType helloWorld

        let! template =
            TextTemplateFactory.Default
            |> TemplateFactory.createTemplate templateContent

        test <@ template.BaseUri = Uri(baseUri) @>
    }

[<Fact>]
let ``TextTemplate.RenderAsync(obj) returns the helloWorld`` () =
    task {
        let templateContent = TemplateContent.ofContent helloWorld

        let! template =
            TextTemplateFactory.Default
            |> TemplateFactory.createTemplate templateContent

        let! result = template |> Template.renderModel (obj())

        test <@ result = helloWorld @>
    }
