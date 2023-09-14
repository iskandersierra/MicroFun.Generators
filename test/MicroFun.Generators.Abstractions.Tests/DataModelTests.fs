module MicroFun.Generators.DataModel.Tests

open System
open System.Linq
open System.Collections.Generic
open Xunit
open FsCheck
open FsCheck.Xunit
open MicroFun.Generators

[<Property>]
let ``IDataAttribute.InnerText() should return the attribute's text`` (NonEmptyString text) =
    // Given
    let attribute =
        { new IDataAttribute with
            member this.Text = text
            member this.Name = Unchecked.defaultof<_>
            member this.Parent = ValueNone
            member this.Tree = Unchecked.defaultof<_>
            member this.Value = Unchecked.defaultof<_> }

    // Given
    let node = attribute :> IDataNode

    // When
    let attributeInnerText = attribute.InnerText()

    // When
    let nodeInnerText = node.InnerText()

    // Then
    Assert.Equal(text, attributeInnerText)

    // Then
    Assert.Equal(text, nodeInnerText)

[<Property>]
let ``IDataValue.InnerText() should return the value's text`` (NonEmptyString text) =
    // Given
    let value =
        { new IDataValue with
            member this.Text = text
            member this.Parent = ValueNone
            member this.Tree = Unchecked.defaultof<_>
            member this.Value = Unchecked.defaultof<_> }

    // Given
    let node = value :> IDataNode

    // When
    let valueInnerText = value.InnerText()

    // When
    let nodeInnerText = node.InnerText()

    // Then
    Assert.Equal(text, valueInnerText)

    // Then
    Assert.Equal(text, nodeInnerText)

[<Property>]
let ``IDataElement.InnerText() should return its content`` (texts: NonEmptyString list) (NonEmptyString attrText) =
    // Given
    let children =
        (texts
        |> Seq.map (fun (NonEmptyString text) ->
            { new IDataValue with
                member this.Text = text
                member this.Parent = ValueNone
                member this.Tree = Unchecked.defaultof<_>
                member this.Value = Unchecked.defaultof<_> } :> IDataLinkedNode)
        |> Seq.toArray)

    // Given
    let expectedText =
        texts
        |> Seq.map (fun x -> x.Get)
        |> String.concat ""

    // Given
    let attribute =
        { new IDataAttribute with
            member this.Text = attrText
            member this.Name = "MyAttr"
            member this.Parent = ValueNone
            member this.Tree = Unchecked.defaultof<_>
            member this.Value = Unchecked.defaultof<_> }

    let attributes = [ attribute ].ToDictionary(fun a -> a.Name)

    // Given
    let value =
        { new IDataElement with
            member this.Children = children
            member this.Parent = ValueNone
            member this.Attributes = attributes
            member this.Elements = Unchecked.defaultof<_>
            member this.Values = Unchecked.defaultof<_>
            member this.Name = Unchecked.defaultof<_>
            member this.Tree = Unchecked.defaultof<_> }

    // Given
    let linkedNode = value :> IDataLinkedNode

    // Given
    let node = value :> IDataNode

    // When
    let valueInnerText = value.InnerText()

    // When
    let linkedNodeInnerText = linkedNode.InnerText()

    // When
    let nodeInnerText = node.InnerText()

    // Then
    Assert.Equal(expectedText, valueInnerText)

    // Then
    Assert.Equal(expectedText, linkedNodeInnerText)

    // Then
    Assert.Equal(expectedText, nodeInnerText)

[<Property>]
let ``IDataLinkedNode seq.InnerText() should return its content`` (texts: NonEmptyString list) =
    // Given
    let collection =
        (texts
        |> Seq.map (fun (NonEmptyString text) ->
            { new IDataValue with
                member this.Text = text
                member this.Parent = ValueNone
                member this.Tree = Unchecked.defaultof<_>
                member this.Value = Unchecked.defaultof<_> } :> IDataLinkedNode))

    // Given
    let expectedText =
        texts
        |> Seq.map (fun x -> x.Get)
        |> String.concat ""

    // When
    let innerText = collection.InnerText()

    // Then
    Assert.Equal(expectedText, innerText)

[<Property>]
let ``IDataTree.InnerText() should return its content`` (texts: NonEmptyString list) =
    // Given
    let roots =
        (texts
        |> Seq.map (fun (NonEmptyString text) ->
            { new IDataValue with
                member this.Text = text
                member this.Parent = ValueNone
                member this.Tree = Unchecked.defaultof<_>
                member this.Value = Unchecked.defaultof<_> } :> IDataLinkedNode)
        |> Seq.toArray)

    // Given
    let expectedText =
        texts
        |> Seq.map (fun x -> x.Get)
        |> String.concat ""

    // Given
    let tree =
        { new IDataTree with
            member this.Roots = roots
            member this.BaseUri = Unchecked.defaultof<_> }

    // When
    let treeInnerText = tree.InnerText()

    // Then
    Assert.Equal(expectedText, treeInnerText)

[<Fact>]
let ``IDataNode.InnerText() on null should fail`` () =
    // Given
    let node = Unchecked.defaultof<IDataNode>

    // Given
    let action = Action(fun () -> node.InnerText() |> ignore)

    // When
    let exn = Assert.ThrowsAny<exn>(action)

    // Then
    Assert.IsAssignableFrom<ArgumentException>(exn)

[<Fact>]
let ``IDataNode.InnerText() on unsupported type should fail`` () =
    // Given
    let node =
        { new IDataNode with
            member this.Parent = Unchecked.defaultof<_>
            member this.Tree = Unchecked.defaultof<_> }

    // Given
    let action = Action(fun () -> node.InnerText() |> ignore)

    // When
    let exn = Assert.ThrowsAny<exn>(action)

    // Then
    Assert.IsAssignableFrom<ArgumentException>(exn)

[<Fact>]
let ``IDataLinkedNode.InnerText() on null should fail`` () =
    // Given
    let node = Unchecked.defaultof<IDataLinkedNode>

    // Given
    let action = Action(fun () -> node.InnerText() |> ignore)

    // When
    let exn = Assert.ThrowsAny<exn>(action)

    // Then
    Assert.IsAssignableFrom<ArgumentException>(exn)

[<Fact>]
let ``IDataLinkedNode.InnerText() on unsupported type should fail`` () =
    // Given
    let node =
        { new IDataLinkedNode with
            member this.Parent = Unchecked.defaultof<_>
            member this.Tree = Unchecked.defaultof<_> }

    // Given
    let action = Action(fun () -> node.InnerText() |> ignore)

    // When
    let exn = Assert.ThrowsAny<exn>(action)

    // Then
    Assert.IsAssignableFrom<ArgumentException>(exn)
