namespace MicroFun.Generators.Commands.Config

open System
open System.Text.Json.Serialization
open System.IO
open System.Threading
open System.Threading.Tasks
open Spectre.Console
open YamlDotNet.Serialization

type ConfigFolderType =
    | Local = 0
    | Intermediate = 1
    | Global = 2

type ConfigFolderInfo =
    { folder: DirectoryInfo
      configType: ConfigFolderType }
    member this.FolderExists = this.folder.Exists


type TemplateProjectData =
    { name: string
      title: string
      description: string
      aliases: string []
      parameters: TemplateParameter [] }

and TemplateParameter =
    { name: string
      title: string
      description: string
      aliases: string []
      [<JsonPropertyName("type"); YamlMember(Alias = "type")>]
      paramType: TemplateParamType
      [<JsonPropertyName("default"); YamlMember(Alias = "default")>]
      defaultValue: string
      values: TemplateEnumValue [] }

and [<JsonConverter(typeof<JsonStringEnumConverter>)>] TemplateParamType =
    | String = 0
    | Enum = 1
    | Model = 2

and TemplateEnumValue =
    { name: string
      title: string
      description: string
      aliases: string [] }

type TemplateFolderInfo =
    { folder: DirectoryInfo
      data: TemplateProjectData }

type ConfigTypeFilter =
    | All = 0
    | Local = 1
    | Global = 2

type ListTemplatesFilter =
    { configTypes: ConfigTypeFilter
      textSearch: string }

type ListTemplatesResult = { templateGroups: TemplateGroup [] }

and TemplateGroup =
    { configFolder: ConfigFolderInfo
      templates: TemplateFolderInfo []
      shadowedTemplates: TemplateFolderInfo [] }


type IConfigRepository =
    abstract GetConfigFolders : ?cancel: CancellationToken -> Task<ConfigFolderInfo []>

    abstract ListTemplates : filter: ListTemplatesFilter * ?cancel: CancellationToken -> Task<ListTemplatesResult>


type ConfigRepositoryOptions() =
    member val CurrentWorkingDirectory: string = null with get, set
    member val GlobalConfigDirectory: string = null with get, set
    member val ConfigFolderName: string = null with get, set


    member this.PropertyToAnsiConsole(console: IAnsiConsole, name: string, value: string) =
        console.MarkupLineInterpolated($"  - [gray]{name}: [/][white]{value}[/]")

    abstract ToAnsiConsole : console: IAnsiConsole -> unit

    default this.ToAnsiConsole(console: IAnsiConsole) =
        console.MarkupLine("[lime]Settings[/]")
        this.PropertyToAnsiConsole(console, "CurrentWorkingDirectory", this.CurrentWorkingDirectory)
        this.PropertyToAnsiConsole(console, "GlobalConfigDirectory", this.GlobalConfigDirectory)
        this.PropertyToAnsiConsole(console, "ConfigFolderName", this.ConfigFolderName)


type ConfigRepository(options: ConfigRepositoryOptions) =
    let templatesFolderName = "templates"

    let configFileName = "config.yaml"

    let getConfigFolders (cancel: CancellationToken option) =
        let globalFolder =
            DirectoryInfo options.GlobalConfigDirectory

        let isSameFolder (dir1: DirectoryInfo) (dir2: DirectoryInfo) =
            Path.GetRelativePath(dir1.FullName, dir2.FullName) = "."

        let rec loop (folder: DirectoryInfo) isCwd folders =
            cancel
            |> Option.iter (fun c -> c.ThrowIfCancellationRequested())

            let configFolder =
                DirectoryInfo(Path.Combine(folder.FullName, options.ConfigFolderName))

            let configType =
                if isSameFolder globalFolder folder then
                    ConfigFolderType.Global
                elif isCwd then
                    ConfigFolderType.Local
                else
                    ConfigFolderType.Intermediate

            let item =
                { folder = configFolder
                  configType = configType }

            let folders' = item :: folders

            if configType = ConfigFolderType.Global then
                folders'
            else
                let parent = folder.Parent

                if parent = null then
                    folders'
                else
                    loop parent false folders'

        let cwd =
            DirectoryInfo options.CurrentWorkingDirectory

        let folders = [] |> loop cwd true |> List.rev

        let folders = folders |> Array.ofList

        folders

    let rec locateTemplateProjects (folder: DirectoryInfo) =
        seq {
            for childDir in folder.GetDirectories() do
                let configFile =
                    childDir.GetFiles(configFileName) |> Array.tryHead

                match configFile with
                | Some configFile ->
                    let data =
                        let deserializer = DeserializerBuilder().Build()
                        let yaml = File.ReadAllText(configFile.FullName)
                        deserializer.Deserialize<TemplateProjectData>(yaml)

                    let item = { folder = childDir; data = data }

                    yield item

                | None -> yield! locateTemplateProjects childDir
        }

    let listTemplates (filter: ListTemplatesFilter) (cancel: CancellationToken option) =
        task {
            let configFolders = getConfigFolders cancel

            let templateGroups = ResizeArray()

            for configFolder in configFolders do
                let templatesFolder =
                    DirectoryInfo(Path.Combine(configFolder.folder.FullName, templatesFolderName))

                if templatesFolder.Exists then
                    let templates =
                        locateTemplateProjects templatesFolder
                        |> Seq.filter applyFilter
                        |> Seq.toArray

                    let templateGroup =
                        { configFolder = configFolder
                          templates = templates
                          shadowedTemplates = Array.empty }

                    templateGroups.Add(templateGroup)

            return { templateGroups = templateGroups.ToArray() }
        }


    interface IConfigRepository with
        member this.GetConfigFolders?cancel = task { return getConfigFolders cancel }

        member this.ListTemplates(filter, ?cancel) = listTemplates filter cancel
