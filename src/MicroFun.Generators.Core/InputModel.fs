namespace MicroFun.Generators


type InputModelContentTypeSelectorLoader(loaders: Map<string, IInputModelLoader>, ?fallback: IInputModelLoader) =
    let loadModel cancel (input: IInputContent) =
        task {
            match loaders |> Map.tryFind input.ContentType, fallback with
            | Some loader, _
            | None, Some loader ->
                return! loader.LoadModel(input, cancel)
            | None, None ->
                return failwithf $"Unrecognized content type: {input.ContentType}"
        }

    interface IInputModelLoader with
        member this.LoadModel(input, cancel) = loadModel cancel input
