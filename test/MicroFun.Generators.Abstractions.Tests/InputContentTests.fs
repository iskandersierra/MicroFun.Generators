module MicroFun.Generators.Abstractions.Tests.InputContentTests

open System
open System.IO
open System.Text
open Xunit
open Swensen.Unquote

open MicroFun.Generators

[<Fact>]
let ``InputContent.tryBuild should set default baseUri if not specified`` () =
    let builder =
        InputContent.builder
        |> InputContent.withText "Hello world!"

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent -> test <@ inputContent.BaseUri = InputContent.DefaultBaseUri @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should use given baseUri if specified`` () =
    let baseUri = Uri("https://example.com/")

    let builder =
        InputContent.builder
        |> InputContent.withBaseUri baseUri
        |> InputContent.withText "Hello world!"

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent -> test <@ inputContent.BaseUri = baseUri @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should set default contentType if not specified`` () =
    let builder =
        InputContent.builder
        |> InputContent.withText "Hello world!"

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent -> test <@ inputContent.ContentType = InputContent.DefaultContentType @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should use given contentType if specified`` () =
    let contentType = "text/html"

    let builder =
        InputContent.builder
        |> InputContent.withContentType contentType
        |> InputContent.withText "Hello world!"

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent -> test <@ inputContent.ContentType = contentType @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should set default encoding if not specified`` () =
    let builder =
        InputContent.builder
        |> InputContent.withText "Hello world!"

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent -> test <@ inputContent.Encoding = InputContent.DefaultEncoding @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should use given encoding if specified`` () =
    let encoding = Encoding.ASCII

    let builder =
        InputContent.builder
        |> InputContent.withEncoding encoding
        |> InputContent.withText "Hello world!"

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent -> test <@ inputContent.Encoding = encoding @>
    | Error _ -> Assert.Fail "Expected Ok"

[<Fact>]
let ``InputContent.tryBuild should fail if no stream or text is specified`` () =
    let builder = InputContent.builder
    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok _ -> Assert.Fail "Expected Error"
    | Error _ -> ()

[<Fact>]
let ``InputContent.tryBuild should fail if both stream and text are specified`` () =
    let builder =
        InputContent.builder
        |> InputContent.withStream (new MemoryStream())
        |> InputContent.withText "Hello world!"

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok _ -> Assert.Fail "Expected Error"
    | Error _ -> ()

[<Fact>]
let ``InputContent.tryBuild should set stream if specified`` () =
    use stream = new MemoryStream()

    let builder =
        InputContent.builder
        |> InputContent.withStream stream

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent -> test <@ inputContent.Stream = stream @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should set properly encoded stream if text is specified`` () =
    let text = "Hello world!"

    let builder =
        InputContent.builder
        |> InputContent.withText text
        |> InputContent.withEncoding Encoding.ASCII

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent ->
        use streamReader =
            new StreamReader(inputContent.Stream, inputContent.Encoding, leaveOpen = true)

        let streamText = streamReader.ReadToEnd()
        test <@ streamText = text @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should prepare to dispose stream`` () =
    let stream = new MemoryStream(Array.zeroCreate 10)

    let builder =
        InputContent.builder
        |> InputContent.withStream stream

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent ->
        inputContent.Dispose()

        let exn =
            Assert.Throws<ObjectDisposedException>(fun () -> inputContent.Stream.Position <- 5L)

        test <@ exn.Message.Contains "closed Stream" @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.tryBuild should prepare to dispose both stream and custom disposable`` () =
    let mutable disposed = false
    let stream = new MemoryStream(Array.zeroCreate 10)
    let dispose = defer (fun () -> disposed <- true)

    let builder =
        InputContent.builder
        |> InputContent.withStream stream
        |> InputContent.withDispose dispose

    let inputContentResult = InputContent.tryBuild builder

    match inputContentResult with
    | Ok inputContent ->
        inputContent.Dispose()
        test <@ disposed @>

        let exn =
            Assert.Throws<ObjectDisposedException>(fun () -> inputContent.Stream.Position <- 5L)

        test <@ exn.Message.Contains "closed Stream" @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``InputContent.build should return if builder is correct`` () =
    let builder =
        InputContent.builder
        |> InputContent.withText "Hello world!"

    let inputContent = InputContent.build builder

    test <@ inputContent.BaseUri = InputContent.DefaultBaseUri @>

[<Fact>]
let ``InputContent.build should fail if builder is incorrect`` () =
    let builder = InputContent.builder

    let exn =
        Assert.Throws<exn>(fun () -> InputContent.build builder |> ignore)

    ()

[<Fact>]
let ``InputContent.ofStream should set stream`` () =
    use stream = new MemoryStream()

    let inputContent = InputContent.ofStream stream

    test <@ inputContent.Stream = stream @>

[<Fact>]
let ``InputContent.ofText should set utf-8 encoded stream`` () =
    let text = "Hello world!"

    let inputContent = InputContent.ofText text

    use streamReader =
        new StreamReader(inputContent.Stream, inputContent.Encoding, leaveOpen = true)

    let streamText = streamReader.ReadToEnd()

    test <@ streamText = text @>
