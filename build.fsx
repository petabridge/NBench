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
                Configuration = configuration }) // "Rebuild"  
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
        | true -> !! "./tests/**/*.Tests.csproj"
        | _ -> !! "./tests/**/*.Tests.csproj" // if you need to filter specs for Linux vs. Windows, do it here

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

Target "NBench" <| fun _ ->
    if (isWindows) then
        // .NET 4.5.2
        let nbenchRunner = findToolInSubPath "NBench.Runner.exe" "src/NBench.Runner/bin/Release/net452/win7-x64"
        let assembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance.WithDependencies/bin/Release/net452/NBench.Tests.Performance.WithDependencies.dll"
        
        let spec = getBuildParam "spec"

        let args = new StringBuilder()
                    |> append assembly
                    |> append (sprintf "output-directory=\"%s\"" outputPerfTests)
                    |> append (sprintf "concurrent=\"%b\"" true)
                    |> append (sprintf "trace=\"%b\"" true)
                    |> toText

        let result = ExecProcess(fun info -> 
            info.FileName <- nbenchRunner
            info.WorkingDirectory <- (Path.GetDirectoryName (FullName nbenchRunner))
            info.Arguments <- args) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
        if result <> 0 then failwithf "NBench.Runner failed. %s %s" nbenchRunner args
    
        // .NET Core
        let netCoreNbenchRunnerProject = "./src/NBench.Runner/NBench.Runner.csproj"
        DotNetCli.Restore
            (fun p ->
                { p with
                    Project = netCoreNbenchRunnerProject
                    AdditionalArgs = ["-r win7-x64"] })
        // build a win7-x64 version of dotnet-nbench.dll so we know we're testing the same architecture
        DotNetCli.Build
            (fun p -> 
                { p with
                    Project = netCoreNbenchRunnerProject
                    Configuration = configuration 
                    Runtime = "win7-x64"
                    Framework = "netcoreapp1.1"})   

        let netCoreNbenchRunner = findToolInSubPath "NBench.Runner.exe" "/src/NBench.Runner/bin/Release/netcoreapp1.1/win7-x64/"
        let netCoreAssembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance.WithDependencies/bin/Release/netstandard1.6/NBench.Tests.Performance.WithDependencies.dll"
        
        let netCoreNbenchRunnerArgs = new StringBuilder()
                                        |> append netCoreAssembly
                                        |> append (sprintf "output-directory=\"%s\"" outputPerfTests)
                                        |> append (sprintf "concurrent=\"%b\"" true)
                                        |> append (sprintf "trace=\"%b\"" true)
                                        |> toText

        let result = ExecProcess(fun info -> 
            info.FileName <- netCoreNbenchRunner
            info.WorkingDirectory <- (Path.GetDirectoryName (FullName netCoreNbenchRunner))
            info.Arguments <- netCoreNbenchRunnerArgs) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
        if result <> 0 then failwithf "NBench.Runner failed. %s %s" netCoreNbenchRunner netCoreNbenchRunnerArgs
    else
        // .NET Core
        let netCoreNbenchRunnerProject = "./src/NBench.Runner.DotNetCli/NBench.Runner.DotNetCli.csproj"
        DotNetCli.Restore
            (fun p ->
                { p with
                    Project = netCoreNbenchRunnerProject
                    AdditionalArgs = ["-r debian.8-x64"] })
        // build a win7-x64 version of dotnet-nbench.dll so we know we're testing the same architecture
        DotNetCli.Build
            (fun p -> 
                { p with
                    Project = netCoreNbenchRunnerProject
                    Configuration = configuration 
                    Runtime = "debian.8-x64"
                    Framework = "netcoreapp1.1"})   
        
        let linuxNbenchRunner =  __SOURCE_DIRECTORY__ @@ "/src/NBench.Runner/bin/Release/netcoreapp1.1/debian.8-x64/NBench.Runner"
        let linuxPerfAssembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance.WithDependencies/bin/Release/netstandard1.6/NBench.Tests.Performance.WithDependencies.dll"
        
        let linuxNbenchRunnerArgs = new StringBuilder()
                                        |> append linuxPerfAssembly
                                        |> append (sprintf "output-directory=\"%s\"" outputPerfTests)
                                        |> append (sprintf "concurrent=\"%b\"" true)
                                        |> append (sprintf "trace=\"%b\"" true)
                                        |> toText

        let result = ExecProcess(fun info -> 
            info.FileName <- linuxNbenchRunner
            info.WorkingDirectory <- __SOURCE_DIRECTORY__ @@ "/src/NBench.Runner.DotNetCli/bin/Release/netcoreapp1.1/debian.8-x64/"
            info.Arguments <- linuxNbenchRunnerArgs) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
        if result <> 0 then failwithf "NBench.Runner failed. %s %s" linuxNbenchRunner linuxNbenchRunnerArgs


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

// build dependencies
"Clean" ==> "RestorePackages" ==> "AssemblyInfo" ==> "Build" ==> "BuildRelease"

// tests dependencies

// nuget dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CreateNuget"
"CreateNuget" ==> "PublishNuget" ==> "Nuget"

// docs
"BuildRelease" ==> "Docfx"

// all
"BuildRelease" ==> "All"
"RunTests" ==> "All"
"NBench" ==> "All"
"Nuget" ==> "All"

RunTargetOrDefault "Help"