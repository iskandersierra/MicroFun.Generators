namespace MicroFun.Generators

open System.Text.RegularExpressions


type SequentialSelectorBuilder =
    { fallback: ITemplateFactory option
      factories: SequentialSelectorBuilderItem list }

and SequentialSelectorBuilderItem =
    { factory: ITemplateFactory
      predicate: string -> bool }


module SequentialSelector =
    let builder = { fallback = None; factories = [] }

    let withFallback fallback builder =
        { builder with fallback = Some fallback }

    let withFactoryPredicate factory predicate (this: SequentialSelectorBuilder) =
        { this with
            factories =
                { factory = factory
                  predicate = predicate }
                :: this.factories }

    let internal predicateOfPredicates (predicates: (string -> bool) seq) =
        let predicates = predicates |> Seq.toArray

        let predicate contentType =
            contentType <> null
            && (predicates |> Seq.exists (fun p -> p contentType))

        predicate

    let withFactoryPredicates factory predicates =
        withFactoryPredicate factory (predicateOfPredicates predicates)

    let internal predicateOfRegex (regex: Regex) =
        let predicate contentType =
            contentType <> null && regex.IsMatch contentType

        predicate

    let withFactoryRegex factory regex =
        withFactoryPredicate factory (predicateOfRegex regex)

    let internal predicateOfRegexes (regexes: Regex seq) =
        let regexes = regexes |> Seq.toArray

        let predicate contentType =
            contentType <> null
            && (regexes |> Seq.exists (fun r -> r.IsMatch contentType))

        predicate

    let withFactoryRegexes factory regexes =
        withFactoryPredicate factory (predicateOfRegexes regexes)

    let build (this: SequentialSelectorBuilder) =
        let factories =
            this.factories |> List.rev |> List.toArray

        let selector =
            { new ITemplateFactorySelector with
                member _.SelectFactory contentType =
                    factories
                    |> Array.tryFind (fun item -> item.predicate contentType)
                    |> Option.map (fun item -> item.factory)
                    |> Option.orElseWith (fun () -> this.fallback)
                    |> Option.defaultValue TextTemplateFactory.Default
            }

        selector

type SequentialSelectorBuilder with
    static member Create() = SequentialSelector.builder

    member this.WithFallback fallback = this |> SequentialSelector.withFallback fallback

    member this.WithFactoryPredicate factory predicate = this |> SequentialSelector.withFactoryPredicate factory predicate
    member this.WithFactoryPredicates factory predicates = this |> SequentialSelector.withFactoryPredicates factory predicates

    member this.WithFactoryRegex factory regex = this |> SequentialSelector.withFactoryRegex factory regex
    member this.WithFactoryRegexes factory regexes = this |> SequentialSelector.withFactoryRegexes factory regexes

    member this.Build() = this |> SequentialSelector.build
