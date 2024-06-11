param (
    [string]$target = "Help"
)

function Clean {
    Remove-Item -Recurse -Force "bin"
    Remove-Item -Recurse -Force "TestResults"
    Remove-Item -Recurse -Force "PerfResults"
    Remove-Item -Recurse -Force "bin/nuget"
    Remove-Item -Recurse -Force "docs/_site"
}

function AssemblyInfo {
    $releaseNotes = Get-Content "RELEASE_NOTES.md" -Raw
    $version = $releaseNotes -match "## \[(\d+\.\d+\.\d+)\]" | Out-Null; $Matches[1]
    $notes = $releaseNotes -split "## \[" | Select-Object -Skip 1
    Set-Content -Path "./src/Directory.Build.props" -Value $version -Force
    Set-Content -Path "./src/Directory.Build.props" -Value $notes -Force
}

function Build {
    dotnet build "*.sln" -c Release
}

function RunTests {
    $projects = Get-ChildItem -Recurse -Filter "*Tests.csproj"
    foreach ($project in $projects) {
        dotnet test $project.FullName -c Release --no-build --logger:trx --logger:"console;verbosity=normal" --results-directory "TestResults" -- -parallel none
    }
}

function NBench {
    $projects = Get-ChildItem -Recurse -Filter "*Tests.Performance.csproj"
    foreach ($project in $projects) {
        dotnet run --project $project.FullName --no-build -c Release -- --output "PerfResults" --concurrent true --trace true --diagnostic
    }
}

function SignPackages {
    $signClientUser = $env:SignClientUser
    $signClientSecret = $env:SignClientSecret
    if ($signClientUser -and $signClientSecret) {
        $assemblies = Get-ChildItem -Recurse -Filter "*.nupkg"
        foreach ($assembly in $assemblies) {
            & SignClient sign --config "appsettings.json" -i $assembly.FullName -r $signClientUser -s $signClientSecret -n "NBench" -d "NBench - performance testing library for .NET" -u "https://nbench.io/"
        }
    } else {
        Write-Host "SignClientUser or SignClientSecret not available. Skipping signing."
    }
}

function CreateNuget {
    $projects = Get-ChildItem -Recurse -Filter "*.csproj" | Where-Object { $_.Name -notmatch "Tests" }
    foreach ($project in $projects) {
        dotnet pack $project.FullName -c Release --include-symbols --no-build -o "bin/nuget"
    }
}

function PublishNuget {
    $packages = Get-ChildItem "bin/nuget/*.nupkg"
    $apiKey = $env:nugetkey
    $source = $env:nugetpublishurl
    if ($apiKey -and $source) {
        foreach ($package in $packages) {
            dotnet nuget push $package.FullName --api-key $apiKey --source $source
        }
    } else {
        Write-Host "nugetkey or nugetpublishurl not available. Skipping publishing."
    }
}

function DocFx {
    dotnet restore "*.sln"
    dotnet build "*.sln" -c Release
    & docfx build "docs/docfx.json"
}

function KillCreatedProcesses {
    dotnet build-server shutdown
}

switch ($target) {
    "Clean" { Clean }
    "AssemblyInfo" { AssemblyInfo }
    "Build" { Build }
    "RunTests" { RunTests }
    "NBench" { NBench }
    "SignPackages" { SignPackages }
    "CreateNuget" { CreateNuget }
    "PublishNuget" { PublishNuget }
    "DocFx" { DocFx }
    "All" {
        Clean
        AssemblyInfo
        Build
        RunTests
        NBench
        CreateNuget
        SignPackages
        PublishNuget
        DocFx
    }
    "Help" {
        Write-Host "usage:"
        Write-Host "./build.ps1 [target]"
        Write-Host ""
        Write-Host " Targets for building:"
        Write-Host " * Build         Builds"
        Write-Host " * Nuget         Create and optionally publish nugets packages"
        Write-Host " * SignPackages  Signs all NuGet packages, provided that the following arguments are passed into the script: SignClientSecret={secret} and SignClientUser={username}"
        Write-Host " * RunTests      Runs tests"
        Write-Host " * All           Builds, run tests, creates and optionally publish nuget packages"
        Write-Host " * DocFx         Creates a DocFx-based website for this solution"
        Write-Host ""
        Write-Host " Other Targets"
        Write-Host " * Help       Display this help"
    }
    default { Write-Host "Invalid target specified." }
}

KillCreatedProcesses