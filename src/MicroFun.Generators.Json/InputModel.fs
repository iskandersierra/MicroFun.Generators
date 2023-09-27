namespace MicroFun.Generators.Json

open System.Dynamic
open System.IO
open Newtonsoft.Json

open MicroFun.Generators


type InputModelDynamicJsonLoader(?settings: JsonSerializerSettings, ?useNonDefaultSettings: bool) =
    let loadModel cancel (input: IInputContent) =
        task {
            let serializer =
                match useNonDefaultSettings, settings with
                | Some true, Some settings -> JsonSerializer.CreateDefault(settings)
                | _, Some settings -> JsonSerializer.Create(settings)
                | Some true, None -> JsonSerializer.CreateDefault()
                | _, None -> JsonSerializer.Create()

            use reader = new StreamReader(input.Stream, input.Encoding, leaveOpen = true)

            use jsonReader = new JsonTextReader(reader)

            let model = serializer.Deserialize(jsonReader, typeof<ExpandoObject>)

            return model
        }

    interface IInputModelLoader with
        member this.LoadModel(input, cancel) = loadModel cancel input
