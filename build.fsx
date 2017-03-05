#I @"tools/FAKE/tools"
#r "FakeLib.dll"

open System
open System.IO
open System.Text

open Fake
open Fake.DotNetCli
open Fake.FileUtils

// Variables
let configuration = "Release"

// Directories
let output = __SOURCE_DIRECTORY__  @@ "build"
let outputTests = output @@ "TestResults"
let outputPerfTests = output @@ "PerfResults"
let outputBinaries = output @@ "binaries"
let outputNuGet = output @@ "nuget"
let outputBinariesNet45 = outputBinaries @@ "net45"
let outputBinariesNetStandard = outputBinaries @@ "netstandard1.6"

Target "Clean" (fun _ ->
    CleanDir output
    CleanDir outputTests
    CleanDir outputPerfTests
    CleanDir outputBinaries
    CleanDir outputNuGet
    CleanDir outputBinariesNet45
    CleanDir outputBinariesNetStandard

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
        DotNetCli.Build
            (fun p ->
                { p with 
                    Project = "./src/NBench.Runner/NBench.Runner.csproj"
                    Framework = "netcoreapp1.0"
                    Configuration = configuration })
        
        let runSingleProjectNetCore project =
            DotNetCli.Build
                (fun p ->
                    { p with
                        Project = project
                        Framework = "netcoreapp1.0"
                        Configuration = configuration })

        let testProjects = (!! "./tests/**/*NBench.Tests*.csproj" 
                            -- "./tests/**/*NBench.PerformanceCounters.Tests.*.csproj")

        testProjects |> Seq.iter (runSingleProjectNetCore)
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
                        -- "./tests/**/*NBench.Tests.End2End.csproj")

        projects |> Seq.iter (log)
        projects |> Seq.iter (runSingleProject)

        let runSingleProjectNet46 project =
            DotNetCli.Test
                (fun p -> 
                    { p with
                        Project = project
                        Framework = "net46"
                        Configuration = configuration})

        let end2EndProject = "./tests/NBench.Tests.End2End/NBench.Tests.End2End.csproj"

        runSingleProjectNet46 end2EndProject

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
                        -- "./tests/**/*NBench.Tests.Assembly.csproj"
                        -- "./tests/**/*NBench.Tests.End2End.csproj")

        projects |> Seq.iter (log)
        projects |> Seq.iter (runSingleProjectNetCore)
)

//--------------------------------------------------------------------------------
// NBench targets
//--------------------------------------------------------------------------------
Target "NBench" <| fun _ ->
    if (isWindows) then
        let nbenchTestPath = findToolInSubPath "NBench.Runner.exe" "tools/NBench.Runner/lib/net45"
        let assembly = __SOURCE_DIRECTORY__ @@ "/tests/NBench.Tests.Performance/bin/Release/net451/NBench.Tests.Performance.dll"
        
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

Target "CopyOutput" (fun _ ->
    // .NET 4.5
    if (isWindows) then
        DotNetCli.Publish
            (fun p -> 
                { p with
                    Project = "./src/NBench/NBench.csproj"
                    Framework = "net45"
                    Output = outputBinariesNet45
                    Configuration = configuration })

    // .NET Core
    DotNetCli.Publish
        (fun p -> 
            { p with
                Project = "./src/NBench/NBench.csproj"
                Framework = "netstandard1.6"
                Output = outputBinariesNetStandard
                Configuration = configuration })
)

Target "CreateNuget" (fun _ ->
    DotNetCli.Pack
        (fun p -> 
            { p with
                Project = "./src/NBench/NBench.csproj"
                Configuration = configuration
                AdditionalArgs = ["--include-symbols"]
                OutputPath = outputNuGet })
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
Target "All" DoNothing

// build dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CopyOutput" ==> "BuildRelease"

// tests dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "RunTests"

// perf dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "NBench"

// nuget dependencies
"Clean" ==> "RestorePackages" ==> "Build" ==> "CreateNuget"

// all
"BuildRelease" ==> "All"
"RunTests" ==> "All"

RunTargetOrDefault "Help"