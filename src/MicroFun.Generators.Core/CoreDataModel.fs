namespace MicroFun.Generators

open System
open System.Collections.Generic
open System.Linq

type DataTreeDescriptor =
    { baseUri: Uri
      getRoots: unit -> DataLinkedNodeDescription seq
      toText: obj -> string
      getAttributes: obj -> NamedValueDescription seq
      getChildren: obj -> DataLinkedNodeDescription seq }

and DataLinkedNodeDescription =
    | DataElementDescription of NamedValueDescription
    | DataValueDescription of value: obj

and NamedValueDescription = { name: string; value: obj }

module DataModel =
    module internal rec Impl =
        let fromNodeDescriptor
            (treeDesc: DataTreeDescriptor)
            (tree: IDataTree)
            (parent: IDataLinkedNode voption)
            (nodeDesc: DataLinkedNodeDescription)
            : IDataLinkedNode =
            match nodeDesc with
            | DataValueDescription value -> DataValue(treeDesc, value, tree, parent)
            | DataElementDescription elemDesc -> DataElement(treeDesc, elemDesc, tree, parent)

        let fromNodeDescriptors
            (treeDesc: DataTreeDescriptor)
            (tree: IDataTree)
            (parent: IDataLinkedNode voption)
            (descs: DataLinkedNodeDescription seq)
            =
            descs
            |> Seq.map (fromNodeDescriptor treeDesc tree parent)
            |> Seq.toList
            :> IReadOnlyList<IDataLinkedNode>

        let fromAttrDescriptor
            (treeDesc: DataTreeDescriptor)
            (parent: IDataLinkedNode)
            (attrDesc: NamedValueDescription)
            : IDataAttribute =
            DataAttribute(treeDesc, attrDesc, parent)

        let fromAttrDescriptors
            (treeDesc: DataTreeDescriptor)
            (parent: IDataLinkedNode)
            (descs: NamedValueDescription seq)
            =
            let attrs =
                descs
                |> Seq.map (fromAttrDescriptor treeDesc parent)

            attrs.ToDictionary(fun a -> a.Name) :> IReadOnlyDictionary<string, IDataAttribute>

        type DataTree(treeDesc: DataTreeDescriptor) as this =
            let roots =
                lazy
                    (treeDesc.getRoots ()
                     |> fromNodeDescriptors treeDesc this ValueNone)

            interface IDataTree with
                member this.BaseUri = treeDesc.baseUri
                member this.Roots = roots.Value

        type DataAttribute(treeDesc: DataTreeDescriptor, attrDesc: NamedValueDescription, parent: IDataLinkedNode) =
            let text = lazy (treeDesc.toText attrDesc.value)
            let tree = parent.Tree
            let parent = ValueSome parent

            interface IDataAttribute with
                member this.Parent = parent
                member this.Tree = tree
                member this.Name = attrDesc.name
                member this.Value = attrDesc.value
                member this.Text = text.Value

        type DataElement
            (
                treeDesc: DataTreeDescriptor,
                elemDesc: NamedValueDescription,
                tree: IDataTree,
                parent: IDataLinkedNode voption
            ) as this =
            let children =
                lazy
                    (treeDesc.getChildren elemDesc.value
                     |> fromNodeDescriptors treeDesc tree (ValueSome this))

            let attributes =
                lazy
                    (treeDesc.getAttributes elemDesc.value
                     |> fromAttrDescriptors treeDesc this)

            interface IDataElement with
                member this.Parent = parent
                member this.Tree = tree
                member this.Name = elemDesc.name
                member this.Children = children.Value
                member this.Attributes = attributes.Value

        type DataValue(treeDesc: DataTreeDescriptor, value: obj, tree: IDataTree, parent: IDataLinkedNode voption) =
            let text = lazy (treeDesc.toText value)

            interface IDataValue with
                member this.Parent = parent
                member this.Tree = tree
                member this.Value = value
                member this.Text = text.Value


    open Impl

    let fromDescriptor (treeDesc: DataTreeDescriptor) = DataTree(treeDesc) :> IDataTree
