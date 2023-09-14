namespace MicroFun.Generators

open System
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.Json.Serialization

//module DataModel =
//    module SystemTextJson =
//        let jsonElementsDescriptor
//            (options: {|
//                baseUri: Uri
//            |})
//            (roots: JsonElement seq) =

//            let mapElement name (elem: JsonElement) : DataLinkedNodeDescription seq =
//                match elem.ValueKind with
//                | JsonValueKind.Object ->
//                    seq {
//                        DataElementDescription { name = name; value = elem }
//                    }

//                | _ -> failwith "Invalid JSON value kind"

//            let roots = lazy (roots |> Seq.collect (mapElement "") |> Seq.toList)

//            {
//                DataTreeDescriptor.baseUri = options.baseUri
//                toText = toText
//                getRoots = fun () -> roots.Value
//                getAttributes = getAttributes
//                getChildren = getChildren
//            }
