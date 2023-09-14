module MicroFun.Generators.CoreDataModel.Tests

open System
open System.Linq
open System.Collections.Generic
open Xunit
open FsCheck
open FsCheck.Xunit
open MicroFun.Generators

let collatzDescriptor (maxValue: int) =
    let newElement (i: int) =
        DataElementDescription { name = "Number"; value = i }

    let newValue (i: int) =
        DataValueDescription i

    { DataTreeDescriptor.baseUri = Uri("https://collatz.example.com", UriKind.Absolute)
      toText = fun o -> $"{o}"
      getRoots = fun () -> Seq.singleton (newElement 1)
      getAttributes =
        fun o ->
            match o with
            | :? int as i ->
                seq {
                    let isEven = i % 2 = 0
                    { name = "IsEven"; value = isEven }
                    { name = "IsOdd"; value = not isEven }
                }
            | _ -> failwith "Invalid value"

      getChildren =
        fun o ->
            match o with
            | :? int as i ->
                seq {
                    yield newValue i

                    let child1 = i * 2
                    if child1 <= maxValue then
                        yield newElement child1

                    if (i - 1) % 3 = 0 then
                        let child2 = (i - 1) / 3
                        if child2 <= maxValue then
                            yield newElement child2
                }
            | _ -> failwith "Invalid value" }

[<Fact>]
let ``DataModel.fromDescriptor should return a tree`` () =
    // Given
    let maxValue = 5

    // Given
    let descriptor = collatzDescriptor maxValue

    // When
    let tree = DataModel.fromDescriptor descriptor

    // Then
    Assert.NotNull tree

    // Given
    let expectedBaseUri = Uri("https://collatz.example.com", UriKind.Absolute)

    // Then
    Assert.Equal(expectedBaseUri, tree.BaseUri)

    // Then
    Assert.Equal("", tree.InnerText())

    let rec assertNode (node: IDataNode) =
        match node with
        | :? IDataValue as value ->
            Assert.Same(tree, value.Tree)
            match value.Parent with
            | ValueNone -> Assert.Fail("Value should have a parent")
            | ValueSome parent ->
                Assert.IsAssignableFrom<IDataElement>(parent) |> ignore
                let intValue = Assert.IsType<int>(value.Value)
                Assert.InRange(intValue, 1, maxValue)
                Assert.Equal($"{intValue}", value.Text)

        | :? IDataElement as elem ->
            Assert.Same(tree, elem.Tree)
            Assert.Equal("Number", elem.Name)

            let firstValue = elem.Values.First()
            let firstIntValue = Assert.IsType<int>(firstValue.Value)
            let isEven = firstIntValue % 2 = 0
            Assert.Equal(2, elem.Attributes.Count)
            let isEvenValue = Assert.IsType<bool>(elem.Attributes.["IsEven"].Value)
            let isOddValue = Assert.IsType<bool>(elem.Attributes.["IsOdd"].Value)
            Assert.Equal(isEven, isEvenValue)
            Assert.NotEqual(isEven, isOddValue)
            if elem.Parent = ValueNone then
                Assert.Equal<string>("1", firstValue.Text)
            else
                Assert.NotEqual<string>("1", firstValue.Text)

            Assert.InRange(elem.Children.Count, 1, 3)
            Assert.InRange(elem.Elements.Count, 0, 2)
            Assert.InRange(elem.Values.Count, 1, 1)
            for child in elem.Children do
                assertNode child

        | _ -> Assert.Fail("Unexpected node type")

    Assert.Collection(tree.Roots, assertNode)

    //Assert.Collection(tree.Roots,
    //    fun node ->
    //        let elem = Assert.IsAssignableFrom<IDataElement>(node)
    //        Assert.Same(tree, elem.Tree)
    //        Assert.Equal(ValueNone, elem.Parent)
    //        Assert.Equal("Number", elem.Name)

    //)
