#I @"tools/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open System.Text

open Fake
open Fake.DotNetCli
open Fake.FileUtils

// Information about the project for Nuget and Assembly info files
let product = "NBench"
let authors = [ "Aaron Stannard" ]
let copyright = "Copyright © 2015-2016"
let company = "Petabridge"
let description = "X-Platform .NET Performance Testing and Measuring Framework"
let tags = ["performance";"benchmarking";"benchmark";"perf testing";"NBench";]
let configuration = "Release"

// Read release notes and version
let parsedRelease =
    File.ReadLines "RELEASE_NOTES.md"
    |> ReleaseNotesHelper.parseReleaseNotes
let envBuildNumber = System.Environment.GetEnvironmentVariable("BUILD_NUMBER") //populated by TeamCity build agent
let buildNumber = if String.IsNullOrWhiteSpace(envBuildNumber) then "0" else envBuildNumber
let version = parsedRelease.AssemblyVersion + "." + buildNumber
let preReleaseVersion = version + "-beta" //suffixes the assembly for pre-releases
let isUnstableDocs = hasBuildParam "unstable"
let isPreRelease = hasBuildParam "nugetprerelease"
let release = if isPreRelease then ReleaseNotesHelper.ReleaseNotes.New(version, version + "-beta", parsedRelease.Notes) else parsedRelease
let isMono = Type.GetType("Mono.Runtime") <> null;

// Directories
let output = __SOURCE_DIRECTORY__  @@ "bin"
let outputTests = __SOURCE_DIRECTORY__ @@ "TestResults"
let outputPerfTests = __SOURCE_DIRECTORY__ @@ "PerfResults"
let outputNuGet = output @@ "nuget"

open AssemblyInfoFile
Target "AssemblyInfo" (fun _ ->
    let version = release.AssemblyVersion

    CreateCSharpAssemblyInfoWithConfig "src/SharedAssemblyInfo.cs" [
        Attribute.Company company
        Attribute.Copyright copyright
        Attribute.Version version
        Attribute.FileVersion version ] <| AssemblyInfoFileConfig(false)
)

Target "Clean" (fun _ ->
    CleanDir output
    CleanDir outputTests
    CleanDir outputPerfTests
    CleanDir outputNuGet
    CleanDirs !! "./**/bin"
    CleanDirs !! "./**/obj"
)

Target "RestorePackages" (fun _ ->
    DotNetCli.Restore
        (fun p -> 
            { p with
                Project = "./NBench.sln"
                NoCache = false })
)

Target "Build" (fun _ ->          
    if (isWindows) then
        let runSingleProject project =
            DotNetCli.Build
                (fun p -> 
                    { p with
                        Project = project
                        Configuration = configuration })   

        let projects = !! "./src/**/*.csproj" ++ "./tests/**/*.csproj"
     
        projects |> Seq.iter (runSingleProject)
    else
        DotNetCli.Build
            (fun p ->
                { p with 
                    Project = "./src/NBench/NBench.csproj"
                    Framework = "netstandard1.6"
                    Configuration = configuration })
        
        let runSingleProjectNetCore project =
            DotNetCli.Build
                (fun p ->
                    { p with
                        Project = project
                        Framework = "netcoreapp1.0"
                        Configuration = configuration })

        let netCoreProjects = (!! "./src/**/*NBench.Runner.DotNetCli.csproj"
                               ++ "./tests/**/*NBench.Tests*.csproj" 
                               -- "./tests/**/*NBench.PerformanceCounters.Tests.*.csproj")

        netCoreProjects |> Seq.iter (runSingleProjectNetCore)
)

Target "RunTests" (fun _ ->
    if (isWindows) then
        let runSingleProject project =
            DotNetCli.Test
                (fun p -> 
                    { p with
                        Project = project
                        Configuration = configuration})      

        let projects = (!! "./tests/**/*NBench.Tests*.csproj"
                        -- "./tests/**/*NBench.Tests.Assembly.csproj")

        projects |> Seq.iter (log)
        projects |> Seq.iter (runSingleProject)

    else
        let runSingleProjectNetCore project =
            DotNetCli.Test
                (fun p -> 
                    { p with
                        Project = project
                        Framework = "netcoreapp1.0"
                        Configuration = configuration})

        let projects = (!! "./tests/**/*NBench.Tests*.csproj" 
                        -- "./tests/**/*NBench.PerformanceCounters.Tests.*.csproj"
                        -- "./tests/**/*NBench.Tests.Performance.csproj"
                        -- "./tests/**/*NBench.Tests.Assembly.csproj")

        projects |> Seq.iter (log)
        projects |> Seq.iter (runSingleProjectNetCore)
)

Target "NBench" <| fun _ ->
    if (isWindows) then
        let nbenchTestPath = findToolInSubPath "NBench.Runner.exe" "tools/NBench.Runner/lib/net45"
        let assembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance/bin/Release/net452/NBench.Tests.Performance.dll"
        
        let spec = getBuildParam "spec"

        let args = new StringBuilder()
                |> append assembly
                |> append (sprintf "output-directory=\"%s\"" outputPerfTests)
                |> append (sprintf "concurrent=\"%b\"" true)
                |> append (sprintf "trace=\"%b\"" true)
                |> toText

        let result = ExecProcess(fun info -> 
            info.FileName <- nbenchTestPath
            info.WorkingDirectory <- (Path.GetDirectoryName (FullName nbenchTestPath))
            info.Arguments <- args) (System.TimeSpan.FromMinutes 15.0) (* Reasonably long-running task. *)
        if result <> 0 then failwithf "NBench.Runner failed. %s %s" nbenchTestPath args
    
    let netCoreNbenchRunner = __SOURCE_DIRECTORY__ @@ "/src/NBench.Runner.DotNetCli/bin/Release/netcoreapp1.0/NBench.Runner.DotNetCli.dll"
    let netCoreAssembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance/bin/Release/netcoreapp1.0/NBench.Tests.Performance.dll"
    DotNetCli.RunCommand
        (fun p ->
            { p with
                TimeOut = TimeSpan.FromMinutes 25.0 })
        (sprintf "%s %s output-directory=\"%s\" concurrent=\"%b\" trace=\"%b\"" netCoreNbenchRunner netCoreAssembly outputPerfTests true true)

Target "CopyOutput" (fun _ ->    
    // .NET 4.5
    if (isWindows) then 
        let projects = [ ("NBench", "./src/NBench/NBench.csproj", "net452");
                         ("NBench.PerformanceCounters", "./src/NBench.PerformanceCounters/NBench.PerformanceCounters.csproj", "net452"); 
                         ("NBench.Runner", "./src/NBench.Runner/NBench.Runner.csproj", "net452") ]

        let publishSingleProjectNet45 project =
            let projectName, projectPath, projectFramework = project
            DotNetCli.Publish
                (fun p -> 
                    { p with
                        Project = projectPath
                        Framework = projectFramework
                        Output = output @@ projectName @@ projectFramework
                        Configuration = configuration })       

    
        projects |> List.iter (fun p -> publishSingleProjectNet45 p)
    
    let netCoreProjects = [ ("NBench", "./src/NBench/NBench.csproj", "netstandard1.6");
                            ("NBench.Runner.DotNetCli", "./src/NBench.Runner.DotNetCli/NBench.Runner.DotNetCli.csproj", "netcoreapp1.0") ]

    let publishSingleProjectNetCoreApp project = 
        let projectName, projectPath, projectFramework = project
        DotNetCli.Publish
            (fun p -> 
                { p with
                    Project = projectPath
                    Framework = projectFramework
                    Output = output @@ projectName @@ projectFramework
                    Configuration = configuration })

    netCoreProjects |> List.iter (fun p -> publishSingleProjectNetCoreApp p)
)

Target "CreateNuget" (fun _ ->
    let nugetProjects = [ "./src/NBench/NBench.csproj"; 
                          "./src/NBench.PerformanceCounters/NBench.PerformanceCounters.csproj";
                          "./src/NBench.Runner/NBench.Runner.csproj" 
                          "./src/NBench.Runner.DotNetCli/NBench.Runner.DotNetCli.csproj" ]

    nugetProjects |> List.iter (fun proj ->
        DotNetCli.Pack
            (fun p -> 
                { p with
                    Project = proj
                    Configuration = configuration
                    AdditionalArgs = ["--include-symbols"]
                    OutputPath = outputNuGet
                    VersionSuffix = version })
        )
)

Target "PublishNuget" (fun _ ->
    let projects = !! "./build/nuget/*.nupkg" -- "./build/nuget/*.symbols.nupkg"
    let apiKey = getBuildParamOrDefault "nugetkey" ""
    let source = getBuildParamOrDefault "nugetpublishurl" ""
    let symbolSource = getBuildParamOrDefault "symbolspublishurl" ""

    let runSingleProject project =
        DotNetCli.RunCommand
            (fun p -> 
                { p with 
                    TimeOut = TimeSpan.FromMinutes 10. })
            (sprintf "nuget push %s --api-key %s --source %s --symbol-source %s" project apiKey source symbolSource)

    projects |> Seq.iter (runSingleProject)
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
      ""
      " Other Targets"
      " * Help       Display this help" 
      ""]

//--------------------------------------------------------------------------------
//  Target dependencies
//--------------------------------------------------------------------------------

Target "BuildRelease" DoNothing
Target "Nuget" DoNothing
Target "All" DoNothing
Target "AllTests" DoNothing

// build dependencies
"Clean" ==> "AssemblyInfo" ==> "RestorePackages" ==> "Build" ==> "CopyOutput" ==> "BuildRelease"

// tests dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "RunTests"

// perf dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "NBench"

// nuget dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CreateNuget"
"CreateNuget" ==> "PublishNuget"
"CreateNuget" ==> "PublishNuget" ==> "Nuget"

// all
"BuildRelease" ==> "All"
"RunTests" ==> "All"
"NBench" ==> "All"
"Nuget" ==> "All"

"BuildRelease" ==> "AllTests"
"RunTests" ==> "AllTests"

RunTargetOrDefault "Help"