# NBench Quickstart Tutorial
Given that NBench users are often on the edge of performance, NBench doesn't ship with any kind of external runner as of NBench 2.0. Instead, NBench is delivered as a [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) - so it can be used in any version of .NET Framework 4.6.1 or higher and any version of .NET Core 2.0 or higher.

## Create a new Console Project
To install NBench, create a new Console Application and add it to your .NET solution:

![Create new Console Application and add it to your solution.](/images/install/create-project.png)

## Install `NBench` Package from NuGet
Install the most recent version of NBench into your brand new Console Application:

```
PS> Install-Package NBench
```

Now, we need to create at least one benchmark class that NBench will run.

## Create One or More Benchmarks
To create a benchmark, create a C# class with a default constructor:

[!code-csharp[CombinedPerfSpecs.cs](../../src/NBench.Tests.Performance/CombinedPerfSpecs.cs)]