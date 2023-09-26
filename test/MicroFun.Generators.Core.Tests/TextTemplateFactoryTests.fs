module MicroFun.Generators.Core.Tests.TextTemplateFactoryTests

open System
open System.IO
open System.Text
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
        let baseUri = Uri("https://example.com/")

        let templateContent =
            InputContent.builder
            |> InputContent.withBaseUri baseUri
            |> InputContent.withContentType contentType
            |> InputContent.withText helloWorld
            |> InputContent.build

        let! template =
            TextTemplateFactory.Default
            |> TemplateFactory.createTemplate templateContent

        test <@ template.BaseUri = baseUri @>
    }

[<Fact>]
let ``TextTemplate.RenderAsync(obj) returns the helloWorld`` () =
    task {
        let templateContent = InputContent.ofText helloWorld

        let! template =
            TextTemplateFactory.Default
            |> TemplateFactory.createTemplate templateContent

        let! result =
            template
            |> Template.renderModel (obj ())
            |> Task.bind OutputContent.toText

        test <@ result = helloWorld @>
    }

[<Fact>]
let ``TextTemplate.RenderAsync(context) returns the helloWorld`` () =
    task {
        let templateContent = InputContent.ofText helloWorld

        let! template =
            TextTemplateFactory.Default
            |> TemplateFactory.createTemplate templateContent

        let context = { TemplateRenderContext.model = obj () }

        let! result =
            template
            |> Template.renderContext context
            |> Task.bind OutputContent.toText

        test <@ result = helloWorld @>
    }

[<Fact>]
let ``TextTemplate.RenderAsync can render template multiple times`` () =
    task {
        use stream =
            new MemoryStream()
            |> Stream.withTextWriter (fun textWriter ->
                textWriter.Write(helloWorld)
                textWriter.Flush())
            |> Stream.withPosition 0L

        let templateContent = InputContent.ofStream stream

        let! template =
            TextTemplateFactory.Default
            |> TemplateFactory.createTemplate templateContent

        let! result1 =
            template
            |> Template.renderModel (obj ())
            |> Task.bind OutputContent.toText

        test <@ result1 = helloWorld @>

        let! result2 =
            template
            |> Template.renderModel (obj ())
            |> Task.bind OutputContent.toText

        test <@ result2 = helloWorld @>
    }
