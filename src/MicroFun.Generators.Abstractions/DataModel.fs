namespace MicroFun.Generators

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Text

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
    abstract Elements : IReadOnlyList<IDataElement>
    abstract Values : IReadOnlyList<IDataValue>

and IDataValue =
    inherit IDataLinkedNode

    abstract Value : obj
    abstract Text : string


[<Extension>]
type DataModelExtensions =
    [<Extension>]
    static member AppendInnerText(this: StringBuilder, node: IDataNode) =
        match node with
        | :? IDataLinkedNode as node -> this.AppendInnerText(node)
        | :? IDataAttribute as node -> this.AppendInnerText(node)
        | _ when obj.ReferenceEquals(node, null) -> invalidArg "node" $"Unsupported null node"
        | _ -> invalidArg "node" $"Unsupported node type: %s{node.GetType().Name}"

    [<Extension>]
    static member InnerText(this: IDataNode) =
        let sb = StringBuilder()
        sb.AppendInnerText(this) |> ignore
        sb.ToString()


    [<Extension>]
    static member AppendInnerText(this: StringBuilder, node: IDataLinkedNode) =
        match node with
        | :? IDataElement as node -> this.AppendInnerText(node)
        | :? IDataValue as node -> this.AppendInnerText(node)
        | _ when obj.ReferenceEquals(node, null) -> invalidArg "node" $"Unsupported null node"
        | _ -> invalidArg "node" $"Unsupported linked node type: %s{node.GetType().Name}"

    [<Extension>]
    static member InnerText(this: IDataLinkedNode) =
        let sb = StringBuilder()
        sb.AppendInnerText(this) |> ignore
        sb.ToString()


    [<Extension>]
    static member AppendInnerText(this: StringBuilder, nodes: IDataLinkedNode seq) =
        for node in nodes do
            this.AppendInnerText(node) |> ignore
        this

    [<Extension>]
    static member InnerText(this: IDataLinkedNode seq) =
        let sb = StringBuilder()
        sb.AppendInnerText(this) |> ignore
        sb.ToString()


    [<Extension>]
    static member AppendInnerText(this: StringBuilder, node: IDataAttribute) = this.Append(node.Text)

    [<Extension>]
    static member InnerText(this: IDataAttribute) = this.Text


    [<Extension>]
    static member AppendInnerText(this: StringBuilder, node: IDataValue) = this.Append(node.Text)

    [<Extension>]
    static member InnerText(this: IDataValue) = this.Text


    [<Extension>]
    static member AppendInnerText(this: StringBuilder, node: IDataElement) =
        this.AppendInnerText(node.Children)
        

    [<Extension>]
    static member InnerText(this: IDataElement) =
        let sb = StringBuilder()
        sb.AppendInnerText(this) |> ignore
        sb.ToString()


    [<Extension>]
    static member AppendInnerText(this: StringBuilder, node: IDataTree) =
        this.AppendInnerText(node.Roots)
        

    [<Extension>]
    static member InnerText(this: IDataTree) =
        let sb = StringBuilder()
        sb.AppendInnerText(this) |> ignore
        sb.ToString()
