#I @"tools/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open System.Text

open Fake
open Fake.DotNetCli
open Fake.FileUtils
open Fake.TaskRunnerHelper

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

// Copied from original NugetCreate target
let nugetDir = output @@ "nuget"
let workingDir = output @@ "build"
let nugetExe = FullName @"./tools/nuget.exe"

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
                        Framework = "netcoreapp1.1"
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
                        -- "./tests/**/*NBench.Tests.Assembly.csproj"
                        -- "./tests/**/*NBench.Tests.Performance.csproj"
                        -- "./tests/**/*NBench.Tests.Performance.WithDependencies.csproj")

        projects |> Seq.iter (log)
        projects |> Seq.iter (runSingleProject)

    else
        let runSingleProjectNetCore project =
            DotNetCli.Test
                (fun p -> 
                    { p with
                        Project = project
                        Framework = "netcoreapp1.1"
                        Configuration = configuration})

        let projects = (!! "./tests/**/*NBench.Tests*.csproj" 
                        -- "./tests/**/*NBench.PerformanceCounters.Tests.*.csproj"
                        -- "./tests/**/*NBench.Tests.Performance.csproj"
                        -- "./tests/**/*NBench.Tests.Performance.WithDependencies.csproj"
                        -- "./tests/**/*NBench.Tests.Assembly.csproj")

        projects |> Seq.iter (log)
        projects |> Seq.iter (runSingleProjectNetCore)
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
        let netCoreNbenchRunnerProject = "./src/NBench.Runner.DotNetCli/NBench.Runner.DotNetCli.csproj"
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

        let netCoreNbenchRunner = findToolInSubPath "dotnet-nbench.exe" "/src/NBench.Runner.DotNetCli/bin/Release/netcoreapp1.1/win7-x64/"
        let netCoreAssembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance.WithDependencies/bin/Release/netcoreapp1.1/NBench.Tests.Performance.WithDependencies.dll"
        
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
        
        let linuxNbenchRunner =  __SOURCE_DIRECTORY__ @@ "/src/NBench.Runner.DotNetCli/bin/Release/netcoreapp1.1/debian.8-x64/dotnet-nbench"
        let linuxPerfAssembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance.WithDependencies/bin/Release/netcoreapp1.1/NBench.Tests.Performance.WithDependencies.dll"
        
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
                            ("NBench.Runner.DotNetCli", "./src/NBench.Runner.DotNetCli/NBench.Runner.DotNetCli.csproj", "netcoreapp1.1") ]

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
                          "./src/NBench.Runner.DotNetCli/NBench.Runner.DotNetCli.csproj" ]

    nugetProjects |> List.iter (fun proj ->
        DotNetCli.Pack
            (fun p -> 
                { p with
                    Project = proj
                    Configuration = configuration
                    AdditionalArgs = ["--include-symbols"]
                    OutputPath = outputNuGet })
        )

    // NBench.Runner.exe NuGet Create

    // Only using this to build NBench.Runner which doesn't need this result
    let getDependencies project = []

     // used to add -pre suffix to pre-release packages
    let getProjectVersion project =
        match project with
        | _ -> release.NugetVersion
    
    let createNugetPackages _ =
        let mutable dirName = 1
        let removeDir dir = 
            let del _ = 
                DeleteDir dir
                not (directoryExists dir)
            runWithRetries del 3 |> ignore

        let getDirName workingDir dirCount =
            workingDir + dirCount.ToString()

        let getReleaseFiles project releaseDir =
            match project with
            | "NBench.Runner" -> 
                !! (releaseDir @@ "*.dll")
                ++ (releaseDir @@ "*.exe")
                ++ (releaseDir @@ "*.pdb")
                ++ (releaseDir @@ "*.xml")
            | _ ->
                !! (releaseDir @@ ".dll")
                ++ (releaseDir @@ ".exe")
                ++ (releaseDir @@ ".pdb")
                ++ (releaseDir @@ ".xml")

        CleanDir workingDir

        ensureDirectory nugetDir
        for nuspec in !! "src/**/*NBench.Runner.nuspec" do
            printfn "Creating nuget packages for %s" nuspec
        
            let project = Path.GetFileNameWithoutExtension nuspec 
            let projectDir = Path.GetDirectoryName nuspec
            let projectFile = (!! (projectDir @@ project + ".*sproj")) |> Seq.head
            let releaseDir = projectDir @@ @"bin\Release\net452\win7-x64"
            let packages = projectDir @@ "packages.config"
            let packageDependencies = if (fileExists packages) then (getDependencies packages) else []
            let dependencies = packageDependencies @ getDependencies project
            let releaseVersion = getProjectVersion project

            let pack outputDir symbolPackage =
                NuGetHelper.NuGet
                    (fun p ->
                        { p with
                            Description = description
                            Authors = authors
                            Copyright = copyright
                            Project =  project
                            Properties = ["Configuration", "Release"]
                            ReleaseNotes = release.Notes |> String.concat "\n"
                            Version = releaseVersion
                            Tags = tags |> String.concat " "
                            OutputPath = outputDir
                            WorkingDir = workingDir
                            SymbolPackage = symbolPackage
                            Dependencies = dependencies })
                    nuspec

            // Copy dll, pdb and xml to libdir = workingDir/lib/net45/
            let libDir = workingDir @@ @"lib\net45"
            printfn "Creating output directory %s" libDir
            ensureDirectory libDir
            CleanDir libDir
            getReleaseFiles project releaseDir
            |> CopyFiles libDir

            // Copy all src-files (.cs and .fs files) to workingDir/src
            let nugetSrcDir = workingDir @@ @"src/"
            CleanDir nugetSrcDir

            let isCs = hasExt ".cs"
            let isFs = hasExt ".fs"
            let isAssemblyInfo f = (filename f).Contains("AssemblyInfo")
            let isSrc f = (isCs f || isFs f) && not (isAssemblyInfo f) 
            CopyDir nugetSrcDir projectDir isSrc
        
            //Remove workingDir/src/obj and workingDir/src/bin
            removeDir (nugetSrcDir @@ "obj")
            removeDir (nugetSrcDir @@ "bin")

            // Create both normal nuget package and symbols nuget package. 
            // Uses the files we copied to workingDir and outputs to nugetdir
            pack nugetDir NugetSymbolPackage.Nuspec

    createNugetPackages()
)

Target "PublishNuget" (fun _ ->
    let projects = !! "./bin/nuget/*.nupkg" -- "./bin/nuget/*.symbols.nupkg"
    let symbols = !! "./bin/nuget/*.symbols.nupkg"

    let apiKey = getBuildParamOrDefault "nugetkey" ""
    let source = getBuildParamOrDefault "nugetpublishurl" ""

    let shouldPushSymbolsPackages = (hasBuildParam "symbolspublishurl") && (hasBuildParam "symbolskey")
    let symbolSource = getBuildParamOrDefault "symbolspublishurl" ""
    let symbolsApiKey = getBuildParamOrDefault "symbolskey" ""

    if (shouldPushSymbolsPackages) then
        let runSingleProject project =
            try
                DotNetCli.RunCommand
                    (fun p -> 
                        { p with 
                            TimeOut = TimeSpan.FromMinutes 10. })
                    (sprintf "nuget push %s --source %s --api-key %s --symbol-source %s --symbol-api-key %s" project source apiKey symbolSource symbolsApiKey)
            with exn ->
                logfn "%s" exn.Message

        symbols |> Seq.iter (runSingleProject)
    else
        let runSingleProject project =
            try
                DotNetCli.RunCommand
                    (fun p -> 
                        { p with 
                            TimeOut = TimeSpan.FromMinutes 10. })
                    (sprintf "nuget push %s --api-key %s --source %s" project apiKey source)
            with exn ->
                logfn "%s" exn.Message

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