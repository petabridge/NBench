#I @"tools/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open System.Text

open Fake
open Fake.DotNetCli
open Fake.DocFxHelper

// Information about the project for Nuget and Assembly info files
let configuration = "Release"

// all of the frameworks we target for builds and packing
let frameworks = ["net452"; "netcoreapp1.1"]

// Read release notes and version
let solutionFile = FindFirstMatchingFile "*.sln" __SOURCE_DIRECTORY__  // dynamically look up the solution
let buildNumber = environVarOrDefault "BUILD_NUMBER" "0"
let hasTeamCity = (not (buildNumber = "0")) // check if we have the TeamCity environment variable for build # set
let preReleaseVersionSuffix = (if (not (buildNumber = "0")) then (buildNumber) else "") + "-beta"
let versionSuffix = 
    match (getBuildParam "nugetprerelease") with
    | "dev" -> preReleaseVersionSuffix
    | _ -> ""

let releaseNotes =
    File.ReadLines "./RELEASE_NOTES.md"
    |> ReleaseNotesHelper.parseReleaseNotes

// Directories
let toolsDir = __SOURCE_DIRECTORY__ @@ "tools"
let output = __SOURCE_DIRECTORY__  @@ "bin"
let outputTests = __SOURCE_DIRECTORY__ @@ "TestResults"
let outputPerfTests = __SOURCE_DIRECTORY__ @@ "PerfResults"
let outputNuGet = output @@ "nuget"

Target "Clean" (fun _ ->
    CleanDir output
    CleanDir outputTests
    CleanDir outputPerfTests
    CleanDir outputNuGet
    CleanDir "docs/_site"
)

Target "AssemblyInfo" (fun _ ->
    XmlPokeInnerText "./src/common.props" "//Project/PropertyGroup/VersionPrefix" releaseNotes.AssemblyVersion    
    XmlPokeInnerText "./src/common.props" "//Project/PropertyGroup/PackageReleaseNotes" (releaseNotes.Notes |> String.concat "\n")
)

Target "RestorePackages" (fun _ ->
    DotNetCli.Restore
        (fun p -> 
            { p with
                Project = solutionFile
                NoCache = false })
)

Target "Build" (fun _ ->          
    DotNetCli.Build
        (fun p -> 
            { p with
                Project = solutionFile
                Configuration = configuration}) // "Rebuild"  
)

//--------------------------------------------------------------------------------
// Tests targets 
//--------------------------------------------------------------------------------
module internal ResultHandling =
    let (|OK|Failure|) = function
        | 0 -> OK
        | x -> Failure x

    let buildErrorMessage = function
        | OK -> None
        | Failure errorCode ->
            Some (sprintf "xUnit2 reported an error (Error Code %d)" errorCode)

    let failBuildWithMessage = function
        | DontFailBuild -> traceError
        | _ -> (fun m -> raise(FailedTestsException m))

    let failBuildIfXUnitReportedError errorLevel =
        buildErrorMessage
        >> Option.iter (failBuildWithMessage errorLevel)

Target "RunTests" (fun _ ->
    let projects = 
        match (isWindows) with 
        | true -> !! "tests/**/*Tests.csproj" 
                   ++ "tests/**/*Tests*.csproj"
                   -- "tests/**/*Tests.Performance.csproj" // skip NBench specs
        | _ -> !! "tests/**/*Tests.csproj" // skip NBench specs // if you need to filter specs for Linux vs. Windows, do it here
                   ++ "tests/**/*Tests*.csproj"
                   -- "tests/**/*PerformanceCounters.Tests*.csproj" // skip performance counter specs on Linux
                   -- "tests/**/*Tests.Performance.csproj" 

    let runSingleProject project =
        let arguments =
            match (hasTeamCity) with
            | true -> (sprintf "xunit -c Release -nobuild -parallel none -teamcity -xml %s_xunit.xml" (outputTests @@ fileNameWithoutExt project))
            | false -> (sprintf "xunit -c Release -nobuild -parallel none -xml %s_xunit.xml" (outputTests @@ fileNameWithoutExt project))

        let result = ExecProcess(fun info ->
            info.FileName <- "dotnet"
            info.WorkingDirectory <- (Directory.GetParent project).FullName
            info.Arguments <- arguments) (TimeSpan.FromMinutes 30.0) 
        
        ResultHandling.failBuildIfXUnitReportedError TestRunnerErrorLevel.DontFailBuild result

    projects |> Seq.iter (log)
    projects |> Seq.iter (runSingleProject)
)

Target "NBenchNet45" <| fun _ ->
    let nbenchProject = FindFirstMatchingFile "NBench.Runner.csproj" (__SOURCE_DIRECTORY__ @@ "src" @@ "NBench.Runner")
    
    // .NET 4.5.2
    let nbenchRunner = "dotnet"
    let assembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance.WithDependencies/bin/Release/net452/NBench.Tests.Performance.WithDependencies.dll"
        
    let spec = getBuildParam "spec"

    let args = new StringBuilder()
                |> append "run"
                |> append "--project"
                |> append nbenchProject // need to unquote this parameter pair or the CLI breaks
                |> append "--framework"
                |> append "net452"
                |> append (sprintf "--configuration %s" configuration)
                |> append assembly
                |> append (sprintf "output-directory=\"%s\"" outputPerfTests)
                |> append (sprintf "concurrent=\"%b\"" true)
                |> append (sprintf "trace=\"%b\"" true)
                |> toText

    let result = ExecProcess(fun info -> 
        info.FileName <- nbenchRunner
        info.Arguments <- args) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
    if result <> 0 then failwithf "NBench.Runner failed. %s %s" nbenchRunner args

Target "NBenchNetCore" <| fun _ ->
    let nbenchProject = FindFirstMatchingFile "NBench.Runner.csproj" (__SOURCE_DIRECTORY__ @@ "src" @@ "NBench.Runner")

    // .NET Core
    let nbenchRunner = "dotnet"
    let assembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance.WithDependencies/bin/Release/netstandard1.6/NBench.Tests.Performance.WithDependencies.dll"
    let spec = getBuildParam "spec"

    let netCoreNbenchRunnerArgs = new StringBuilder()
                                    |> append "run"
                                    |> append "--project"
                                    |> append nbenchProject // need to unquote this parameter pair or the CLI breaks
                                    |> append "--framework"
                                    |> append "netcoreapp1.1"
                                    |> append (sprintf "--configuration %s" configuration)
                                    |> append assembly
                                    |> append (sprintf "output-directory=\"%s\"" outputPerfTests)
                                    |> append (sprintf "concurrent=\"%b\"" true)
                                    |> append (sprintf "trace=\"%b\"" true)
                                    |> toText

    let result = ExecProcess(fun info -> 
        info.FileName <- nbenchRunner
        info.Arguments <- netCoreNbenchRunnerArgs) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
    if result <> 0 then failwithf "NBench.Runner failed. %s %s" nbenchRunner netCoreNbenchRunnerArgs 

//--------------------------------------------------------------------------------
// Nuget targets 
//--------------------------------------------------------------------------------

let overrideVersionSuffix (project:string) =
    match project with
    | _ -> versionSuffix // add additional matches to publish different versions for different projects in solution
Target "CreateNuget" (fun _ ->    
    let projects = !! "src/**/*.csproj" 
                   -- "tests/**/*Tests.csproj" // Don't publish unit tests
                   -- "tests/**/*Tests*.csproj"
                   -- "src/**/*.Runner.csproj" // Don't publish runners

    // Update NuSpec for DotNetCli prior to pack operation
    CopyFile "./src/NBench.Runner.DotNetCli/dotnet-nbench.nuspec" "./src/NBench.Runner.DotNetCli/dotnet-nbench.nuspec.template"
    let commonPropsVersionPrefix = XMLRead true "./src/common.props" "" "" "//Project/PropertyGroup/VersionPrefix" |> Seq.head
    let versionReplacement = List.ofSeq [ "@version@", commonPropsVersionPrefix + (if (not (versionSuffix = "")) then ("-" + versionSuffix) else "") ]
    let releaseNotesReplacement = List.ofSeq ["@release_notes@", (releaseNotes.Notes |> String.concat "\n")]

    TemplateHelper.processTemplates versionReplacement [ "./src/NBench.Runner.DotNetCli/dotnet-nbench.nuspec" ]
    TemplateHelper.processTemplates releaseNotesReplacement [ "./src/NBench.Runner.DotNetCli/dotnet-nbench.nuspec" ]

    let runSingleProject project =
        DotNetCli.Pack
            (fun p -> 
                { p with
                    Project = project
                    Configuration = configuration
                    AdditionalArgs = ["--include-symbols --no-build"]
                    VersionSuffix = overrideVersionSuffix project
                    OutputPath = outputNuGet })

    projects |> Seq.iter (runSingleProject)

    // clean up the temporary folder
    DeleteFile "./src/NBench.Runner.DotNetCli/dotnet-nbench.nuspec"
)

Target "CreateRunnerNuGet" (fun _ ->
    // uses the template file to create a temporary .nuspec file with the correct version
    CopyFile "./src/NBench.Runner/NBench.Runner.nuspec" "./src/NBench.Runner/NBench.Runner.nuspec.template"
    let commonPropsVersionPrefix = XMLRead true "./src/common.props" "" "" "//Project/PropertyGroup/VersionPrefix" |> Seq.head
    let versionReplacement = List.ofSeq [ "@version@", commonPropsVersionPrefix + (if (not (versionSuffix = "")) then ("-" + versionSuffix) else "") ]
    let releaseNotesReplacement = List.ofSeq ["@release_notes@", (releaseNotes.Notes |> String.concat "\n")]
    TemplateHelper.processTemplates versionReplacement [ "./src/NBench.Runner/NBench.Runner.nuspec" ]
    TemplateHelper.processTemplates releaseNotesReplacement [ "./src/NBench.Runner/NBench.Runner.nuspec" ]


    let executableProjects = !! "./src/**/NBench.Runner.csproj"

    executableProjects |> Seq.iter (fun project ->
        frameworks |> Seq.iter (fun framework ->
            DotNetCli.Publish
                (fun p -> 
                    { p with
                        Project = project
                        Configuration = configuration
                        Runtime = "win7-x64"
                        Framework = framework
                        VersionSuffix = overrideVersionSuffix project } ) 
                    )
                )
    
    executableProjects |> Seq.iter (fun project ->  
        DotNetCli.Pack
            (fun p -> 
                { p with
                    Project = project
                    Configuration = configuration
                    AdditionalArgs = ["--include-symbols"]
                    VersionSuffix = overrideVersionSuffix project
                    OutputPath = outputNuGet } )
    )

    DeleteFile "./src/NBench.Runner/NBench.Runner.nuspec"
)

Target "PublishNuget" (fun _ ->
    let projects = !! "./bin/nuget/*.nupkg" -- "./bin/nuget/*.symbols.nupkg"
    let apiKey = getBuildParamOrDefault "nugetkey" ""
    let source = getBuildParamOrDefault "nugetpublishurl" ""
    let symbolSource = getBuildParamOrDefault "symbolspublishurl" ""
    let shouldPublishSymbolsPackages = not (symbolSource = "")

    if (not (source = "") && not (apiKey = "") && shouldPublishSymbolsPackages) then
        let runSingleProject project =
            DotNetCli.RunCommand
                (fun p -> 
                    { p with 
                        TimeOut = TimeSpan.FromMinutes 10. })
                (sprintf "nuget push %s --api-key %s --source %s --symbol-source %s" project apiKey source symbolSource)

        projects |> Seq.iter (runSingleProject)
    else if (not (source = "") && not (apiKey = "") && not shouldPublishSymbolsPackages) then
        let runSingleProject project =
            DotNetCli.RunCommand
                (fun p -> 
                    { p with 
                        TimeOut = TimeSpan.FromMinutes 10. })
                (sprintf "nuget push %s --api-key %s --source %s" project apiKey source)

        projects |> Seq.iter (runSingleProject)
)

//--------------------------------------------------------------------------------
// Documentation 
//--------------------------------------------------------------------------------  
Target "DocFx" (fun _ ->
    DotNetCli.Restore (fun p -> { p with Project = solutionFile })
    DotNetCli.Build (fun p -> { p with Project = solutionFile; Configuration = configuration })

    let docsPath = "./docs"

    DocFx (fun p -> 
                { p with 
                    Timeout = TimeSpan.FromMinutes 30.0; 
                    WorkingDirectory  = docsPath; 
                    DocFxJson = docsPath @@ "docfx.json" })
)

//--------------------------------------------------------------------------------
// Help 
//--------------------------------------------------------------------------------

Target "Help" <| fun _ ->
    List.iter printfn [
      "usage:"
      "./build.ps1 [target]"
      ""
      " Targets for building:"
      " * Build      Builds"
      " * Nuget      Create and optionally publish nugets packages"
      " * RunTests   Runs tests"
      " * All        Builds, run tests, creates and optionally publish nuget packages"
      " * DocFx      Creates a DocFx-based website for this solution"
      ""
      " Other Targets"
      " * Help       Display this help" 
      ""]

//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

Target "BuildRelease" DoNothing
Target "All" DoNothing
Target "Nuget" DoNothing
Target "NBench" DoNothing

// build dependencies
"Clean" ==> "RestorePackages" ==> "AssemblyInfo" ==> "Build" ==> "BuildRelease"

// tests dependencies

// nuget dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CreateNuget"
"CreateRunnerNuGet" ==> "CreateNuget" ==> "PublishNuget" ==> "Nuget"

// docs
"BuildRelease" ==> "Docfx"

// NBench
"NBenchNet45" ==> "NBench"
"NBenchNetCore" ==> "NBench"

// all
"BuildRelease" ==> "All"
"RunTests" ==> "All"
"NBench" ==> "All"
"Nuget" ==> "All"

RunTargetOrDefault "Help"