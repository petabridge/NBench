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
    let runSingleProject project =
        DotNetCli.Build
            (fun p -> 
                { p with
                    Project = project
                    Configuration = configuration 
                    AdditionalArgs = ["--no-incremental"]}) // "Rebuild"  

    let assemblies = !! "./src/**/*.csproj" 
                     ++ "./tests/**/*.csproj"
                     -- "./src/**/NBench.Runner.DotNetCli.csproj" // no longer building this proj
                     -- "./src/**/NBench.Runner.csproj"
     
    assemblies |> Seq.iter (runSingleProject)
    
    let runners = !! "./src/**/NBench.Runner.csproj"

    // build win7-x64 target 
    runners |> Seq.iter (fun x ->
        DotNetCli.Build
            (fun p ->
                { p with
                    Project = x
                    Configuration = configuration
                    AdditionalArgs = ["--no-incremental"]}))
    
    // make sure we build a debian.8-x64 runtime as well
    // must restore for debian before building for debian
    runners |> Seq.iter (fun x ->
        DotNetCli.Restore
            (fun p ->
                { p with
                    Project = x
                    AdditionalArgs = ["-r debian.8-x64"] }))

    // build for debian
    runners |> Seq.iter (fun x ->
        DotNetCli.Build
            (fun p ->
                { p with
                    Project = x
                    Configuration = configuration
                    Runtime = "debian.8-x64"
                    AdditionalArgs = ["--no-incremental"]}))
)

Target "RunTests" (fun _ ->
    let sampleBenchmarProjects = !! "./tests/**/NBench.Tests.Performance.csproj"
                                 ++ "./tests/**/NBench.Tests.Performance.WithDependencies.csproj"
                                 ++ "./tests/**/NBench.Tests.Assembly.csproj"
    
    sampleBenchmarProjects |> Seq.iter (fun proj ->
        DotNetCli.Build
            (fun p ->
                { p with
                    Project = proj
                    Configuration = configuration
                    AdditionalArgs = ["--no-incremental"]}))

    let runSingleProject project =
        DotNetCli.RunCommand
            (fun p -> 
                { p with 
                    WorkingDir = (Directory.GetParent project).FullName
                    TimeOut = TimeSpan.FromMinutes 10. })
                (sprintf "xunit -parallel none -teamcity -xml %s_xunit.xml" (outputTests @@ fileNameWithoutExt project))   

    let projects = 
        match (isWindows) with 
        | true -> (!! "./tests/**/*NBench.Tests*.csproj" 
                    -- "./tests/**/*NBench.PerformanceCounters.Tests.*.csproj"
                    -- "./tests/**/*NBench.Tests.Performance.csproj"
                    -- "./tests/**/*NBench.Tests.Performance.WithDependencies.csproj"
                    -- "./tests/**/*NBench.Tests.Assembly.csproj")
        | _ -> (!! "./tests/**/*NBench.Tests*.csproj" 
                -- "./tests/**/*NBench.Tests.Reporting.csproj"
                -- "./tests/**/*NBench.PerformanceCounters.Tests.*.csproj"
                -- "./tests/**/*NBench.Tests.Performance.csproj"
                -- "./tests/**/*NBench.Tests.Performance.WithDependencies.csproj"
                -- "./tests/**/*NBench.Tests.Assembly.csproj")

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
    
        //// .NET Core
        //let netCoreNbenchRunnerProject = "./src/NBench.Runner.DotNetCli/NBench.Runner.DotNetCli.csproj"
        //DotNetCli.Restore
        //    (fun p ->
        //        { p with
        //            Project = netCoreNbenchRunnerProject
        //            AdditionalArgs = ["-r win7-x64"] })
        //// build a win7-x64 version of dotnet-nbench.dll so we know we're testing the same architecture
        //DotNetCli.Build
        //    (fun p -> 
        //        { p with
        //            Project = netCoreNbenchRunnerProject
        //            Configuration = configuration 
        //            Runtime = "win7-x64"
        //            Framework = "netcoreapp1.1"})   

        let netCoreNbenchRunner = findToolInSubPath "dotnet-nbench.exe" "/src/NBench.Runner.DotNetCli/bin/Release/netcoreapp1.1/win7-x64/"
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
        
        let linuxNbenchRunner =  __SOURCE_DIRECTORY__ @@ "/src/NBench.Runner.DotNetCli/bin/Release/netcoreapp1.1/debian.8-x64/dotnet-nbench"
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

//--------------------------------------------------------------------------------
// Nuget targets 
//--------------------------------------------------------------------------------
module Nuget = 
    // add NBench dependency for other projects
    let getNBenchDependencies project =
        match project with
        | "NBench.PerformanceCounters" -> ["NBench", release.NugetVersion]
        | _ -> []

    // used to add -pre suffix to pre-release packages
    let getProjectVersion project =
      match project with
      | _ -> release.NugetVersion

open Nuget

//--------------------------------------------------------------------------------
// Clean nuget directory
Target "CleanNuget" (fun _ ->
    CleanDir nugetDir
)

//--------------------------------------------------------------------------------
// Pack nuget for all projects
// Publish to nuget.org if nugetkey is specified

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
            !! (releaseDir @@ project + ".dll")
            ++ (releaseDir @@ "NBench.dll")
            ++ (releaseDir @@ project + ".exe")
            ++ (releaseDir @@ project + ".pdb")
            ++ (releaseDir @@ "NBench.pdb")
            ++ (releaseDir @@ project + ".xml")
        | "NBench.Runner.DotNetCli" ->
            !! (releaseDir @@ "*")
        | _ ->
            !! (releaseDir @@ project + ".dll")
            ++ (releaseDir @@ project + ".exe")
            ++ (releaseDir @@ project + ".pdb")
            ++ (releaseDir @@ project + ".xml")

    CleanDir workingDir

    let releaseDirLookup nuspecFile =
        match Path.GetFileName nuspecFile with
        | "NBench.Runner.nuspec" -> @"bin\Release\net452\win7-x64"
        | "NBench.Runner.DotNetCli.nuspec" -> @"bin\Release\netcoreapp1.1\win7-x64"
        | _ -> @"bin\Release\"

    ensureDirectory nugetDir
    for nuspec in !! "src/**/*.nuspec" do
        printfn "Creating nuget packages for %s" nuspec
        
        let project = Path.GetFileNameWithoutExtension nuspec 
        let projectDir = Path.GetDirectoryName nuspec
        let projectFile = (!! (projectDir @@ project + ".*sproj")) |> Seq.head
        let releaseDir = projectDir @@ (releaseDirLookup nuspec)
        let packages = projectDir @@ "packages.config"
        let packageDependencies = if (fileExists packages) then (getNBenchDependencies packages) else []
        let dependencies = packageDependencies @ getNBenchDependencies project
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
        |> CopyFiles libDir // TODO: this is where to glob all releases together

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


let publishNugetPackages _ = 
    let rec publishPackage url accessKey trialsLeft packageFile =
        let tracing = enableProcessTracing
        enableProcessTracing <- false
        let args p =
            match p with
            | (pack, key, "") -> sprintf "push \"%s\" %s" pack key
            | (pack, key, url) -> sprintf "push \"%s\" %s -source %s" pack key url

        tracefn "Pushing %s Attempts left: %d" (FullName packageFile) trialsLeft
        try 
            let result = ExecProcess (fun info -> 
                    info.FileName <- nugetExe
                    info.WorkingDirectory <- (Path.GetDirectoryName (FullName packageFile))
                    info.Arguments <- args (packageFile, accessKey,url)) (System.TimeSpan.FromMinutes 1.0)
            enableProcessTracing <- tracing
            if result <> 0 then failwithf "Error during NuGet symbol push. %s %s" nugetExe (args (packageFile, "key omitted",url))
        with exn -> 
            if (trialsLeft > 0) then (publishPackage url accessKey (trialsLeft-1) packageFile)
            else raise exn
    let shouldPushNugetPackages = hasBuildParam "nugetkey"
    let shouldPushSymbolsPackages = (hasBuildParam "symbolspublishurl") && (hasBuildParam "symbolskey")
    
    if (shouldPushNugetPackages || shouldPushSymbolsPackages) then
        printfn "Pushing nuget packages"
        if shouldPushNugetPackages then
            let normalPackages= 
                !! (nugetDir @@ "*.nupkg") 
                -- (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            for package in normalPackages do
                try
                    publishPackage (getBuildParamOrDefault "nugetpublishurl" "") (getBuildParam "nugetkey") 3 package
                with exn ->
                    printfn "%s" exn.Message

        if shouldPushSymbolsPackages then
            let symbolPackages= !! (nugetDir @@ "*.symbols.nupkg") |> Seq.sortBy(fun x -> x.ToLower())
            for package in symbolPackages do
                try
                    publishPackage (getBuildParam "symbolspublishurl") (getBuildParam "symbolskey") 3 package
                with exn ->
                    printfn "%s" exn.Message

Target "Nuget" <| fun _ -> 
    createNugetPackages()
    publishNugetPackages()

Target "CreateNuget" <| fun _ -> 
    createNugetPackages()

Target "PublishNuget" <| fun _ -> 
    publishNugetPackages()

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
Target "All" DoNothing
Target "AllTests" DoNothing

// build dependencies
"Clean" ==> "AssemblyInfo" ==> "RestorePackages" ==> "Build" ==> "CopyOutput" ==> "BuildRelease"

// tests dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "RunTests"

// perf dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "NBench"

// nuget dependencies
"CleanNuget" ==> "BuildRelease" ==> "Nuget"

// all
"BuildRelease" ==> "All"
"RunTests" ==> "All"
"NBench" ==> "All"
"Nuget" ==> "All"

"BuildRelease" ==> "AllTests"
"RunTests" ==> "AllTests"

RunTargetOrDefault "Help"