namespace MicroFun.Generators

open System
open System.Text.RegularExpressions

[<Struct>]
type TemplateFactorySelectorItem =
    { factory: ITemplateFactory
      predicate: string -> bool }

type TemplateFactorySelectorConfig =
    { fallback: ITemplateFactory option
      factories: TemplateFactorySelectorItem list }


module TemplateFactorySelectorConfig =
    let empty = { fallback = None; factories = [] }

    let inline withFallback fallback config =
        { config with fallback = Some fallback }

    let inline withDefaultFallback config = { config with fallback = None }

    let withPredicate factory predicate config =
        { config with
            factories =
                { factory = factory
                  predicate = predicate }
                :: config.factories }

    let withPredicates factory predicates =
        let predicate contentType =
            contentType <> null
            && (predicates |> Seq.exists (fun p -> p contentType))

        withPredicate factory predicate

    let withRegex factory (regex: Regex) =
        let predicate contentType =
            contentType <> null && regex.IsMatch contentType

        withPredicate factory predicate

    let withRegexes factory (regexes: Regex seq) =
        let predicate contentType =
            contentType <> null
            && (regexes
                |> Seq.exists (fun r -> r.IsMatch contentType))

        withPredicate factory predicate


type TemplateFactorySelector(config) =
    let fallback =
        config.fallback
        |> Option.defaultValue TextTemplateFactory.Default

    let factories =
        config.factories |> List.rev |> Seq.toArray

    member _.SelectFactory(contentType: string) =
        if String.IsNullOrWhiteSpace(contentType) then
            fallback
        else
            factories
            |> Seq.tryFind (fun item -> item.predicate (contentType))
            |> Option.map (fun item -> item.factory)
            |> Option.defaultValue fallback


type SelectorTemplateFactory(selector) =
    new(config) = SelectorTemplateFactory(TemplateFactorySelector(config))

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                let factory =
                    selector.SelectFactory(content.contentType)

                return! factory.CreateTemplateAsync(content, cancel)
            }
