module MicroFun.Generators.Core.Tests.SelectorTemplateFactoryTests

open System.IO
open System.Text.RegularExpressions
open Xunit
open Swensen.Unquote

open MicroFun.Generators

//// TemplateFactorySelectorConfig

//[<Fact>]
//let ``TemplateFactorySelectorConfig.empty should return empty config`` () =
//    let config = TemplateFactorySelectorConfig.empty

//    test <@ config.fallback = None @>
//    test <@ config.factories |> List.isEmpty @>

//[<Fact>]
//let ``TemplateFactorySelectorConfig.withFallback should set the fallback factory`` () =
//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withFallback TextTemplateFactory.Default

//    test <@ config.fallback = Some TextTemplateFactory.Default @>

//[<Fact>]
//let ``TemplateFactorySelectorConfig.withDefaultFallback should set the fallback factory to None`` () =
//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withFallback TextTemplateFactory.Default
//        |> TemplateFactorySelectorConfig.withDefaultFallback

//    test <@ config.fallback = None @>

//[<Fact>]
//let ``TemplateFactorySelectorConfig.withPredicate should prepend to factories`` () =
//    let predicate1 =
//        fun contentType -> contentType = "text/plain"

//    let factory1 = TextTemplateFactory()

//    let predicate2 =
//        fun contentType -> contentType = "text/html"

//    let factory2 = TextTemplateFactory()

//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withPredicate factory1 predicate1
//        |> TemplateFactorySelectorConfig.withPredicate factory2 predicate2

//    match config.factories with
//    | [ { factory = f2; predicate = p2 }; { factory = f1; predicate = p1 } ] ->
//        test <@ f2 = factory2 @>
//        test <@ obj.ReferenceEquals(p2, predicate2) @>
//        test <@ f1 = factory1 @>
//        test <@ obj.ReferenceEquals(p1, predicate1) @>

//    | _ -> Assert.Fail("Expected exactly two factories in given order")

//[<Fact>]
//let ``TemplateFactorySelectorConfig.withPredicates should combine predicates`` () =
//    let predicate1 =
//        fun contentType -> contentType = "text/plain"

//    let predicate2 =
//        fun contentType -> contentType = "text/html"

//    let factory = TextTemplateFactory()

//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withPredicates factory [ predicate1; predicate2 ]

//    match config.factories with
//    | [ { predicate = actualPredicate } ] ->
//        test <@ actualPredicate "text/plain" @>
//        test <@ actualPredicate "text/html" @>
//        test <@ not (actualPredicate "text/xml") @>

//    | _ -> Assert.Fail("Expected exactly one factory")

//[<Fact>]
//let ``TemplateFactorySelectorConfig.withRegex should apply regex`` () =
//    let regex = Regex("^text/.+$")
//    let factory = TextTemplateFactory()

//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withRegex factory regex

//    match config.factories with
//    | [ { predicate = actualPredicate } ] ->
//        test <@ actualPredicate "text/plain" @>
//        test <@ actualPredicate "text/html" @>
//        test <@ actualPredicate "text/vnd+xml" @>
//        test <@ not (actualPredicate "application/json") @>

//    | _ -> Assert.Fail("Expected exactly one factory")

//[<Fact>]
//let ``TemplateFactorySelectorConfig.withRegexes should combine regexes`` () =
//    let regex1 = Regex("^text/.+$")
//    let regex2 = Regex("^.+/(.+\+)?json$")
//    let factory = TextTemplateFactory()

//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withRegexes factory [ regex1; regex2 ]

//    match config.factories with
//    | [ { predicate = actualPredicate } ] ->
//        test <@ actualPredicate "text/plain" @>
//        test <@ actualPredicate "text/html" @>
//        test <@ actualPredicate "application/json" @>
//        test <@ actualPredicate "text/vnd+json" @>
//        test <@ not (actualPredicate "application/xml") @>

//    | _ -> Assert.Fail("Expected exactly one factory")


//// TemplateFactorySelector

//[<Fact>]
//let ``TemplateFactorySelector.SelectFactory should select fallback factory`` () =
//    let xmlPredicate =
//        fun contentType -> contentType = "text/xml"

//    let xmlFactory = TextTemplateFactory()

//    let htmlPredicate =
//        fun contentType -> contentType = "text/html"

//    let htmlFactory = TextTemplateFactory()

//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withPredicate xmlFactory xmlPredicate
//        |> TemplateFactorySelectorConfig.withPredicate htmlFactory htmlPredicate

//    let selector = TemplateFactorySelector(config)

//    let factory = selector.SelectFactory("text/other")

//    test <@ factory = TextTemplateFactory.Default @>

//[<Fact>]
//let ``TemplateFactorySelector.SelectFactory should select expected factory`` () =
//    let xmlPredicate =
//        fun contentType -> contentType = "text/xml"

//    let xmlFactory = TextTemplateFactory()

//    let htmlPredicate =
//        fun contentType -> contentType = "text/html"

//    let htmlFactory = TextTemplateFactory()

//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withPredicate xmlFactory xmlPredicate
//        |> TemplateFactorySelectorConfig.withPredicate htmlFactory htmlPredicate

//    let selector = TemplateFactorySelector(config)

//    let factory = selector.SelectFactory("text/html")

//    test <@ factory = htmlFactory @>

//// SelectorTemplateFactory

//let createSampleSelectorTemplateFactory () =
//    let xmlPredicate =
//        fun contentType -> contentType = "text/xml"

//    let xmlFactory: ITemplateFactory =
//        MockTextTemplateFactory(TemplateContent.ofText "Is XML")

//    let htmlPredicate =
//        fun contentType -> contentType = "text/html"

//    let htmlFactory: ITemplateFactory =
//        MockTextTemplateFactory(TemplateContent.ofText "Is HTML")

//    let fallbackFactory: ITemplateFactory =
//        MockTextTemplateFactory(TemplateContent.ofText "Is fallback")

//    let config =
//        TemplateFactorySelectorConfig.empty
//        |> TemplateFactorySelectorConfig.withPredicate xmlFactory xmlPredicate
//        |> TemplateFactorySelectorConfig.withPredicate htmlFactory htmlPredicate
//        |> TemplateFactorySelectorConfig.withFallback fallbackFactory

//    let factory: ITemplateFactory = SelectorTemplateFactory(config)

//    factory

//[<Fact>]
//let ``SelectorTemplateFactory.SelectFactory should select fallback factory`` () =
//    task {
//        let factory = createSampleSelectorTemplateFactory ()

//        let templateContent =
//            "[green]Hello World![/]"
//            |> TemplateContent.ofText
//            |> TemplateContent.withContentType "text/markup"

//        let! template =
//            factory
//            |> TemplateFactory.createTemplate templateContent

//        let! result = template |> Template.renderModel (obj ())

//        test <@ result = "Is fallback" @>
//    }

//[<Fact>]
//let ``SelectorTemplateFactory.SelectFactory should select first factory`` () =
//    task {
//        let factory = createSampleSelectorTemplateFactory ()

//        let templateContent =
//            "[green]Hello World![/]"
//            |> TemplateContent.ofText
//            |> TemplateContent.withContentType "text/xml"

//        let! template =
//            factory
//            |> TemplateFactory.createTemplate templateContent

//        let! result = template |> Template.renderModel (obj ())

//        test <@ result = "Is XML" @>
//    }

//[<Fact>]
//let ``SelectorTemplateFactory.SelectFactory should select next factory`` () =
//    task {
//        let factory = createSampleSelectorTemplateFactory ()

//        let templateContent =
//            "[green]Hello World![/]"
//            |> TemplateContent.ofText
//            |> TemplateContent.withContentType "text/html"

//        let! template =
//            factory
//            |> TemplateFactory.createTemplate templateContent

//        let! result = template |> Template.renderModel (obj ())

//        test <@ result = "Is HTML" @>
//    }
