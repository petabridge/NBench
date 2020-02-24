#### v2.0.0 February 24 2020
NBench 2.0.0 is a major departure from NBench 1.2 and preceding versions, and these changes were done in order to support NBench's future as a cutting-edge, cross-platform performance testing and macro benchmarking framework:

- `dotnet nbench` and `NBench.Runner` are both now deprecated - [NBench is now run from directly inside a console application created by end-users](https://nbench.io/articles/quickstart.html). This makes it easier to configure, debug, and create benchmarks on new .NET Core platforms without having to wait for additional instrumentation or tooling from NBench itself.
- NBench no longer supports .NET Framework explicitly; moving forward NBench will only support .NET Standard 2.0 and later (.NET Framework 4.6.1 and greater or .NET Core 2.0 and greater.)
- We've added a new documentation website for NBench: https://nbench.io/
- NBench now supports configuration as code through the [`TestPackage` class](https://nbench.io/api/NBench.Sdk.TestPackage.html).

For a full set of changes, [please see the NBench 2.0.0 milestone on Github](https://github.com/petabridge/NBench/milestone/3).