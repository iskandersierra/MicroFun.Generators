module MicroFun.Generators.Scriban.Tests.ScribanTemplateTests

open System
open System.Dynamic
open Newtonsoft.Json
open Xunit
open Swensen.Unquote
open FSharp.Interop.Dynamic

open MicroFun.Generators
open MicroFun.Generators.Scriban

// ScribanTemplate.Scriban

let scribanExtensionRegexSamples () =
    seq {
        let fileName = "template"

        for scribanPart in [ "sbn"; "scriban"; "SBN" ] do
            yield $"{fileName}.{scribanPart}"
            let subType = "html"

            for subTypeGlue in [ ""; "-"; "." ] do
                yield $"{fileName}.{scribanPart}{subTypeGlue}{subType}"
    }
    |> Seq.map (fun fileName -> [| fileName :> obj |])

// https://marketplace.visualstudio.com/items?itemName=xoofx.scriban
[<Theory>]
[<MemberData(nameof (scribanExtensionRegexSamples))>]
let ``ScribanTemplate.Scriban.extensionRegex should accept all possible extensions`` (fileName: string) =
    let match' =
        ScribanTemplate.Scriban.extensionRegex.Match(fileName)

    test <@ match'.Success @>


[<Theory>]
[<InlineData(".sbn")>]
[<InlineData(".scriban")>]
[<InlineData("template.scbn")>]
[<InlineData("template.scrib")>]
[<InlineData("template.hmtl")>]
[<InlineData("scriban.hmtl")>]
let ``ScribanTemplate.Scriban.extensionRegex should reject other extensions`` (fileName: string) =
    let match' =
        ScribanTemplate.Scriban.extensionRegex.Match(fileName)

    test <@ not match'.Success @>


let scribanContentTypeRegexSamples () =
    seq {
        for groupName in [ "text"; "application" ] do
            for scribanPart in [ "sbn"; "scriban" ] do
                yield $"{groupName}/{scribanPart}"
                let subType = "html"
                yield $"{groupName}/{subType}+{scribanPart}"
    }
    |> Seq.map (fun contentType -> [| contentType :> obj |])

[<Theory>]
[<MemberData(nameof (scribanContentTypeRegexSamples))>]
let ``ScribanTemplate.Scriban.contentTypeRegex should accept all possible contentTypes`` (fileName: string) =
    let match' =
        ScribanTemplate.Scriban.contentTypeRegex.Match(fileName)

    test <@ match'.Success @>


[<Theory>]
[<InlineData("text/html")>]
[<InlineData("application/html")>]
[<InlineData("text/scriban+html")>]
[<InlineData("text/sbn+html")>]
let ``ScribanTemplate.Scriban.contentTypeRegex should reject other contentTypes`` (fileName: string) =
    let match' =
        ScribanTemplate.Scriban.contentTypeRegex.Match(fileName)

    test <@ not match'.Success @>


// ScribanTemplate.DotLiquid

let dotLiquidExtensionRegexSamples () =
    seq {
        let fileName = "template"

        for dotLiquidPart in [ "liquid"; "dotliquid"; "dot-liquid" ] do
            yield $"{fileName}.{dotLiquidPart}"
            let subType = "html"

            for subTypeGlue in [ ""; "-"; "." ] do
                yield $"{fileName}.{dotLiquidPart}{subTypeGlue}{subType}"
    }
    |> Seq.map (fun fileName -> [| fileName :> obj |])

[<Theory>]
[<MemberData(nameof (dotLiquidExtensionRegexSamples))>]
let ``ScribanTemplate.DotLiquid.extensionRegex should accept all possible extensions`` (fileName: string) =
    let match' =
        ScribanTemplate.DotLiquid.extensionRegex.Match(fileName)

    test <@ match'.Success @>


[<Theory>]
[<InlineData(".dotliquid")>]
[<InlineData("template.dliquid")>]
[<InlineData("template.dtl")>]
[<InlineData("template.hmtl")>]
[<InlineData("dotliquid.hmtl")>]
let ``ScribanTemplate.DotLiquid.extensionRegex should reject other extensions`` (fileName: string) =
    let match' =
        ScribanTemplate.DotLiquid.extensionRegex.Match(fileName)

    test <@ not match'.Success @>


let dotLiquidContentTypeRegexSamples () =
    seq {
        for groupName in [ "text"; "application" ] do
            for dotLiquidPart in [ "liquid"; "dotliquid"; "dot-liquid" ] do
                yield $"{groupName}/{dotLiquidPart}"
                let subType = "html"
                yield $"{groupName}/{subType}+{dotLiquidPart}"
    }
    |> Seq.map (fun contentType -> [| contentType :> obj |])

[<Theory>]
[<MemberData(nameof (dotLiquidContentTypeRegexSamples))>]
let ``ScribanTemplate.DotLiquid.contentTypeRegex should accept all possible contentTypes`` (fileName: string) =
    let match' =
        ScribanTemplate.DotLiquid.contentTypeRegex.Match(fileName)

    test <@ match'.Success @>


[<Theory>]
[<InlineData("text/html")>]
[<InlineData("application/html")>]
[<InlineData("text/dotliquid+html")>]
[<InlineData("text/liquid+html")>]
let ``ScribanTemplate.DotLiquid.contentTypeRegex should reject other contentTypes`` (fileName: string) =
    let match' =
        ScribanTemplate.DotLiquid.contentTypeRegex.Match(fileName)

    test <@ not match'.Success @>


// ScribanTemplateFactory

[<Fact>]
let ``ScribanTemplateFactory.Default should be non-null`` () =
    let factory = ScribanTemplateFactory.Default

    Assert.NotNull factory

[<Fact>]
let ``ScribanTemplateFactory.CreateTemplateAsync should return a scriban-backed template`` () =
    task {
        let baseUri = Uri("https://www.example.com")

        let inputContent =
            InputContentBuilder
                .Create()
                .WithText(@"Hello {{if !name; name=""world""; end }}{{ name }}!")
                .WithContentType("text/plain+scriban")
                .WithBaseUri(baseUri)
                .Build()

        let! template =
            ScribanTemplateFactory.Default
            |> TemplateFactory.createTemplate inputContent

        Assert.NotNull template
        Assert.Equal(baseUri, template.BaseUri)

        let! result =
            template
            |> Template.renderModel {| name = "Alice" |}
            |> Task.bind OutputContent.toText

        test <@ result = "Hello Alice!" @>
    }

[<Fact>]
let ``ScribanTemplateFactory.CreateTemplateAsync should fail from a dotliquid template`` () =
    task {
        let baseUri = Uri("https://www.example.com")

        let inputContent =
            InputContentBuilder
                .Create()
                .WithText(@"Hello {% if name %}{{name}}{% else %}world{% endif %}!")
                .WithContentType("text/plain+liquid")
                .WithBaseUri(baseUri)
                .Build()

        do!
            Assert.ThrowsAnyAsync (fun () ->
                task {
                    let! template =
                        ScribanTemplateFactory.Default
                        |> TemplateFactory.createTemplate inputContent

                    let! result =
                        template
                        |> Template.renderModel {| name = "Alice" |}
                        |> Task.bind OutputContent.toText

                    test <@ result = "Hello Alice!" @>
                })
            |> Task.ignore
    }

[<Fact>]
let ``ScribanTemplate.RenderAsync should render anonymous objects`` () =
    task {
        let inputContent =
            InputContent.ofText @"Hello {{ name }}<{{address.email}}>!"

        let! template =
            ScribanTemplateFactory.Default
            |> TemplateFactory.createTemplate inputContent

        let model =
            {| name = "Alice"
               address = {| email = "alice@examle.com" |} |}

        let! result =
            template
            |> Template.renderModel model
            |> Task.bind OutputContent.toText

        test <@ result = "Hello Alice<alice@examle.com>!" @>
    }

[<Fact>]
let ``ScribanTemplate.RenderAsync should render Dictionary`` () =
    task {
        let inputContent =
            InputContent.ofText @"Hello {{ name }}<{{address.email}}>!"

        let! template =
            ScribanTemplateFactory.Default
            |> TemplateFactory.createTemplate inputContent

        let model = dict [
            "name", "Alice" :> obj
            "address", {| email = "alice@examle.com" |} :> obj
        ]

        let! result =
            template
            |> Template.renderModel model
            |> Task.bind OutputContent.toText

        test <@ result = "Hello Alice<alice@examle.com>!" @>
    }

[<Fact>]
let ``ScribanTemplate.RenderAsync should render ExpandoObject`` () =
    task {
        let inputContent =
            InputContent.ofText @"Hello {{ name }}<{{address.email}}>!"

        let! template =
            ScribanTemplateFactory.Default
            |> TemplateFactory.createTemplate inputContent

        let model = ExpandoObject()
        model?name <- "Alice"
        model?address <- ExpandoObject()
        model?address?email <- "alice@examle.com"

        let! result =
            template
            |> Template.renderModel model
            |> Task.bind OutputContent.toText

        test <@ result = "Hello Alice<alice@examle.com>!" @>
    }

[<Fact>]
let ``ScribanTemplate.RenderAsync should render Json dynamic object`` () =
    task {
        let inputContent =
            InputContent.ofText @"Hello {{ name }}<{{address.email}}>!"

        let! template =
            ScribanTemplateFactory.Default
            |> TemplateFactory.createTemplate inputContent

        let json = """
        {
            "name": "Alice",
            "address": {
                "email": "alice@examle.com"
            }
        }
        """

        let model = JsonConvert.DeserializeObject<ExpandoObject>(json)

        let! result =
            template
            |> Template.renderModel model
            |> Task.bind OutputContent.toText

        test <@ result = "Hello Alice<alice@examle.com>!" @>
    }


// DotLiquidTemplateFactory

[<Fact>]
let ``DotLiquidTemplateFactory.Default should be non-null`` () =
    let factory = DotLiquidTemplateFactory.Default

    Assert.NotNull factory

[<Fact>]
let ``DotLiquidTemplateFactory.CreateTemplateAsync should return a dotliquid-backed template`` () =
    task {
        let baseUri = Uri("https://www.example.com")

        let inputContent =
            InputContentBuilder
                .Create()
                .WithText(@"Hello {% if name %}{{name}}{% else %}world{% endif %}!")
                .WithContentType("text/plain+liquid")
                .WithBaseUri(baseUri)
                .Build()

        let! template =
            DotLiquidTemplateFactory.Default
            |> TemplateFactory.createTemplate inputContent

        Assert.NotNull template
        Assert.Equal(baseUri, template.BaseUri)

        let! result =
            template
            |> Template.renderModel {| name = "Alice" |}
            |> Task.bind OutputContent.toText

        test <@ result = "Hello Alice!" @>
    }

[<Fact>]
let ``DotLiquidTemplateFactory.CreateTemplateAsync should fail from a scriban template`` () =
    task {
        let baseUri = Uri("https://www.example.com")

        let inputContent =
            InputContentBuilder
                .Create()
                .WithText(@"Hello {{if !name; name=""world""; end }}{{ name }}!")
                .WithContentType("text/plain+scriban")
                .WithBaseUri(baseUri)
                .Build()

        do!
            Assert.ThrowsAnyAsync (fun () ->
                task {
                    let! template =
                        DotLiquidTemplateFactory.Default
                        |> TemplateFactory.createTemplate inputContent

                    let! result =
                        template
                        |> Template.renderModel {| name = "Alice" |}
                        |> Task.bind OutputContent.toText

                    test <@ result = "Hello Alice!" @>
                })
            |> Task.ignore
    }
