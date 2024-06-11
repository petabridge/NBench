$projects = Get-ChildItem -Recurse -Filter "*Tests.Performance.csproj"
foreach ($project in $projects) {
    dotnet run --project $project.FullName --no-build -c Release -- --output "PerfResults" --concurrent true --trace true --diagnostic
}
