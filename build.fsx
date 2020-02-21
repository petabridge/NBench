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
let frameworks = ["net452"; "netcoreapp1.0"; "netcoreapp2.0"; "netcoreapp2.1";]

// Read release notes and version
let solutionFile = FindFirstMatchingFile "*.sln" __SOURCE_DIRECTORY__  // dynamically look up the solution
let buildNumber = environVarOrDefault "BUILD_NUMBER" "0"
let hasTeamCity = (not (buildNumber = "0")) // check if we have the TeamCity environment variable for build # set
let preReleaseVersionSuffix = (if (not (buildNumber = "0")) then (buildNumber) else "") + ("-beta" + DateTime.UtcNow.Ticks.ToString())
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
    ActivateFinalTarget "KillCreatedProcesses"
    DotNetCli.Restore
        (fun p -> 
            { p with
                Project = solutionFile
                NoCache = false })
)

Target "Build" (fun _ -> 
    ActivateFinalTarget "KillCreatedProcesses"         
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
                   -- "tests/**/*Tests.Performance.**.csproj" // skip NBench specs
        | _ -> !! "tests/**/*Tests.csproj" // skip NBench specs // if you need to filter specs for Linux vs. Windows, do it here
                   ++ "tests/**/*Tests*.csproj"
                   -- "tests/**/*PerformanceCounters.Tests*.csproj" // skip performance counter specs on Linux
                   -- "tests/**/*Tests.Performance.csproj" 
                   -- "tests/**/*Tests.Performance.**.csproj" // skip NBench specs

    let runSingleProject project =
        let arguments =
            match (hasTeamCity) with
            | true -> (sprintf "test -c Release --no-build --logger:trx --logger:\"console;verbosity=normal\" --results-directory %s -- -parallel none -teamcity" (outputTests))
            | false -> (sprintf "test -c Release --no-build --logger:trx --logger:\"console;verbosity=normal\" --results-directory %s -- -parallel none" (outputTests))

        let result = ExecProcess(fun info ->
            info.FileName <- "dotnet"
            info.WorkingDirectory <- (Directory.GetParent project).FullName
            info.Arguments <- arguments) (TimeSpan.FromMinutes 30.0) 
        
        ResultHandling.failBuildIfXUnitReportedError TestRunnerErrorLevel.Error result  

    projects |> Seq.iter (log)
    projects |> Seq.iter (runSingleProject)
)

Target "NBench" DoNothing

//Target "NBench" <| fun _ ->
//    let nbenchTestAssemblies = !! "./tests/**/*Tests.Performance.csproj" 
//    let dotnetNBenchDll = findToolInSubPath "dotnet-nbench.dll" "./src/**/bin/Release/netcoreapp2.0"

//    nbenchTestAssemblies |> Seq.iter(fun project -> 
//        let args = new StringBuilder()
//                |> append dotnetNBenchDll // need to unquote this parameter pair or the CLI breaks
//                |> append "--project"
//                |> append (filename project)
//                |> append "--output"
//                |> append outputPerfTests
//                |> append "--concurrent" 
//                |> append "true"
//                |> append "--trace"
//                |> append "true"
//                |> append "--diagnostic"
//                |> append "--no-build"
//                |> toText

//        let result = ExecProcess(fun info -> 
//            info.FileName <- "dotnet"
//            info.WorkingDirectory <- (Directory.GetParent project).FullName
//            info.Arguments <- args) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
//        if result <> 0 then failwithf "NBench.Runner failed. %s %s" "dotnet" args
//    )

    

//--------------------------------------------------------------------------------
// Nuget targets 
//--------------------------------------------------------------------------------

let overrideVersionSuffix (project:string) =
    match project with
    | _ -> versionSuffix // add additional matches to publish different versions for different projects in solution

Target "CreateNuspecs" (fun _ ->
    let nuspecs = !! "src/**/*.nuspec.template"

    // For each individual .nuspec.template file, need to inject some credentials
    let commonPropsVersionPrefix = XMLRead true "./src/common.props" "" "" "//Project/PropertyGroup/VersionPrefix" |> Seq.head
    let versionReplacement = List.ofSeq [ "@version@", commonPropsVersionPrefix + (if (not (versionSuffix = "")) then ("-" + versionSuffix) else "") ]
    let releaseNotesReplacement = List.ofSeq ["@release_notes@", (releaseNotes.Notes |> String.concat "\n")]

    nuspecs |> Seq.iter (fun template ->
        let fileInfo = new FileInfo(template);
        let nuspec = (Path.Combine(fileInfo.DirectoryName, fileInfo.Name.Replace(".template", "")))

        // Create the temporary .nuspec file
        CopyFile nuspec template

        TemplateHelper.processTemplates versionReplacement [ nuspec ]
        TemplateHelper.processTemplates releaseNotesReplacement [ nuspec ]
    )
)

Target "CleanupNuspecs" (fun _ ->
    let nuspecs = !! "src/**/*.nuspec"

    nuspecs |> Seq.iter (fun nuspec ->
        DeleteFile nuspec
    )
)

Target "CreateNuget" (fun _ ->    
    let projects = !! "src/**/*.csproj" 
                   -- "tests/**/*Tests.csproj" // Don't publish unit tests
                   -- "tests/**/*Tests*.csproj"
                   -- "src/**/*.Runner.csproj" // Don't publish runners  

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
)

Target "CreateRunnerNuGet" (fun _ ->
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

FinalTarget "KillCreatedProcesses" (fun _ ->
    log "Killing processes started by FAKE:"
    startedProcesses |> Seq.iter (fun (pid, _) -> logfn "%i" pid)
    killAllCreatedProcesses()
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

// build dependencies
"Clean" ==> "RestorePackages" ==> "AssemblyInfo" ==> "Build" ==> "BuildRelease"

// tests dependencies
"Build" ==> "RunTests"

// nuget dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CreateNuget"
"CreateNuspecs" ==> "CreateRunnerNuGet" ==> "CreateNuget" ==> "CleanupNuspecs" ==> "PublishNuget" ==> "Nuget"

// docs
"BuildRelease" ==> "Docfx"

// NBench
"CreateRunnerNuGet" ==> "NBench"

// all
"BuildRelease" ==> "All"
"RunTests" ==> "All"
"NBench" ==> "All"
"Nuget" ==> "All"

RunTargetOrDefault "Help"