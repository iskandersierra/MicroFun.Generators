namespace MicroFun.Generators

open System
open System.Collections.Generic

type IDataNode =
    abstract Parent : IDataLinkedNode voption
    abstract Tree : IDataTree

and IDataLinkedNode =
    inherit IDataNode


and IDataTree =
    abstract BaseUri : Uri
    abstract Roots : IReadOnlyList<IDataLinkedNode>

and IDataAttribute =
    inherit IDataNode

    abstract Name : string

    abstract Value : obj
    abstract Text : string


and IDataElement =
    inherit IDataLinkedNode

    abstract Name : string

    abstract Attributes : IReadOnlyDictionary<string, IDataAttribute>
    abstract Children : IReadOnlyList<IDataLinkedNode>

and IDataValue =
    inherit IDataLinkedNode

    abstract Value : obj
    abstract Text : string


[<AutoOpen>]
module DataModelExtensions =
    open System.Text

    type IDataNode with
        member this.InnerText() =
            let sb = StringBuilder()

            let rec loop (node: IDataNode) =
                match node with
                | :? IDataElement as elem ->
                    for child in elem.Children do
                        loop child

                | :? IDataValue as value -> sb.Append(value.Text) |> ignore

                | _ -> ()

            loop this

            sb.ToString()

    type IDataTree with
        member this.InnerText() =
            let sb = StringBuilder()

            for root in this.Roots do
                sb.Append(root.InnerText()) |> ignore

            sb.ToString()
