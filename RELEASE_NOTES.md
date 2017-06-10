#### v1.0.3 June 7 2017

This release resolves issues with NuGet deployment of the dedicated NBench.Runner.DotNetCli runner that was originally designed to be used for .NET Core projects.  The issue is detailed by [#200](https://github.com/petabridge/NBench/issues/200) and resolved with PR [#201](https://github.com/petabridge/NBench/pull/201).

Important breaking change:

NBench.Runner.DotNetCli is being deprecated (temporarily) as the supported means of running the NBench runner with a benchmark assembly that targets .NET Core.  Instead, the original [NBench.Runner](https://www.nuget.org/packages/NBench.Runner/) will come packaged with 2 additional executables that are compatible with .NET Core.  Originally, per the instructions on the README, to run the **.NET 4.5.2** you would run the following commands:

```
PS> Install-Package NBench.Runner
PS> .\packages\NBench.Runner\NBench.Runner.exe .\src\bin\Debug\MyPerfTests.dll output-directory="C:\Perf
```

Since the new NBench.Runner NuGet package ships the additional .NET Core runner, the folder structure of the downloaded runner is as follows:

lib/
    net452/
        NBench.Runner.exe
    netcoreapp1.1/
        win7-x64/
            NBench.Runner.exe
        debian8-x64/
            NBench.Runner

The above way to run the .NET 4.5.2 runner, hence, changes to:

```
PS> Install-Package NBench.Runner
PS> .\packages\NBench.Runner\lib\net452\NBench.Runner.exe .\src\bin\Debug\net452\MyPerfTests.dll output-directory="C:\Perf
```

For .NET Core support (meaning running a benchmark that has been targeted for `netcoreapp1.1` or `netstandard1.6`, you will run the appropriate NBench.Runner.exe for your architecture:

```
PS> Install-Package NBench.Runner
PS> .\packages\NBench.Runner\lib\netcoreapp1.1\win7-x64\NBench.Runner.exe .\src\bin\Debug\netcoreapp1.1\MyPerfTests.dll output-directory="C:\Perf
```

or, on Debian 8:

```
PS> Install-Package NBench.Runner
PS> .\packages\NBench.Runner\lib\netcoreapp1.1\debian8-x64\NBench.Runner.exe .\src\bin\Debug\netcoreapp1.1\MyPerfTests.dll output-directory="C:\Perf
```

Plans will be made to re-introduce support for NBench.Runner.DotNetCli which allows for the usage of NBench as a `DotNetCliToolReference`.

#### v1.0.2 May 31 2017

This release resolves issues: [#182](https://github.com/petabridge/NBench/issues/182) and [#192](https://github.com/petabridge/NBench/issues/192) relating to the NBench.Runner.DotNetCli (the NBench Runner that can execute benchmarks for assemblies that target .NET Core).  The root cause was that the runner was unable to execute .NET Core benchmarks that had external dependencies.

Other changes include:

- Integration tests to validate that the above issue is resolved
- Upgrade to xUnit .NET Core CLI runner (v2.3.0-beta2-build3683) for all NBench unit tests
- Update .NET Core runtime targets to `netcoreapp1.1`

If you are using the .NET Core CLI runner for NBench and encountered this issue, please be sure to update your `<DotNetCliToolReference>` to v1.0.2:

  ```
  <ItemGroup>
    <DotNetCliToolReference Include="NBench.Runner.DotNetCli" Version="1.0.2" />
  </ItemGroup>
  ```

#### v1.0.1 March 29 2017

This release resolves an issue with the v1.0.0 NuGet release for NBench.Runner in which clients installing the package via `Install-Package NBench.Runner` were not getting the NBench.dll dependency for NBench.Runner.exe.  NBench.dll is now compiled into NBench.Runner.exe.

#### v1.0.0 March 14 2017

NBench v1.0.0 represents support for .NET Standard 1.6 for the NBench core library and a .NET Core 1.0 version of the NBench runner.

This release introduces a breaking change to NBench:

- NBench core library .NET Framework target increased from **4.5 -> 4.5.2**.  Client projects must target 4.5.2 or greater.

How to use the .NET Core NBench runner:

- In your .NET Core performance test project, add the following dependency element to the .csproj file:

  ```
  <ItemGroup>
    <DotNetCliToolReference Include="NBench.Runner.DotNetCli" Version="1.0.0" />
  </ItemGroup>
  ```

- Save the .csproj file (if using Visual Studio 2017) or run `dotnet restore` in the project location.

- From a command prompt within the project's parent directory, run `dotnet nbench project_name.dll arguments...`

You can [see the full list of changes in NBench 1.0.0 here](https://github.com/petabridge/NBench/milestone/2)

#### v0.3.4  December 15 2016
NBench v0.3.4 is a bugfix for [`RunMode.ThroughPut` benchmarks where we regularly had false negatives on asserting number of operations per second.](https://github.com/petabridge/NBench/issues/153). This patch fixes this issue.

#### 0.3.3 December 07 2016
NBench v0.3.3 includes a handful of bug fixes, but also enables TeamCity output formatting for NBench specifications.

To enable TeamCity output formatting explicitly, pass in the following argument to the `NBench.Runner.exe`

```
NBench.Runner.exe [assembly names] [teamcity=true]
```

#### 0.3.1 August 15 2016
NBench v0.3.1 introduces full Mono support for cross-platform benchmarks to both `NBench.Runner` as well as the core `NBench` library.

No major new changes have been added feature wise, but we've worked around issues specific to Linux permissions and have tailored the core NBench package to be able to run benchmarks on any platform.

Here's an example of some [side-by-side NBench benchmarks on Mono vs. .NET 4.6.2](https://gist.github.com/Aaronontheweb/228507db024fe00ee88b5e5f14e6d679).

NBench.PerformanceCounters is platform-specific to Windows and thus can't be supported on Linux.

[Read the full list of changes in NBench v0.3.1 here](https://github.com/petabridge/NBench/milestone/1).

#### 0.3.0 May 24 2016
This release introduces some breaking changes to NBench:

**Tracing**
The biggest feature included in this release is the addition of tracing support, which is exposed directly to end-users so they can capture trace events and include them in the output produced by NBench.

You can access the `IBenchmarkTrace` object through the `BenchmarkContext` passed into any of your `PerfSetup`, `PerfBenchmark`, or `PerfCleanup` methods like such:

```csharp
public class TracingBenchmark
{
   
    [PerfSetup]
    public void Setup(BenchmarkContext context)
    {
        context.Trace.Debug(SetupTrace);
    }

    [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = IterationCount, RunTimeMilliseconds = 1000)]
    [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
    [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
    public void Run1(BenchmarkContext context)
    {
        context.Trace.Debug(RunTrace);
    }

    [PerfCleanup]
    public void Cleanup(BenchmarkContext context)
    {
        context.Trace.Info(CleanupTrace);
    }
}
```

`NBench.Runner.exe` now takes a `trace=true|false` commandline argument, which will enable the new tracing feature introduced in this release.

**Tracing is disabled by default**.

**Skippable Warmups**
You can now elect to skip warmups altogether for your specs. This feature is particularly useful for long-running iteration benchmarks, which are often used for stress tests. Warmups don't add any value here.

Here's how you can skip warmups:

```csharp
[PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = IterationCount, RunTimeMilliseconds = 1000, SkipWarmups = true)]
[MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
[MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
public void Run1(BenchmarkContext context)
{
    context.Trace.Debug(RunTrace);
}
```

Just set `SkipWarmups = true` on your `PerfBenchmark` attribute wherever you wish to skip a warmup.

**Foreground thread is no longer given high priority when concurrent mode is on**.

If you are running the `NBench.Runner` with `concurrent=true`, we no longer give the main foreground thread high priority as this resulted in some unfair scheduling during concurrent tests. All threads within the `NBench.Runner` process all share the same priority now.

**Markdown reports include additional data**
All markdown reports now include:

* The concurrency setting for NBench
* The tracing setting for NBench
* A flag indicating if warmups were skipped or not

All of these were added in order to make it easy for end-users reading the reports to know what the NBench settings were at the time the report was produced.

#### 0.2.2 May 03 2016
Warmup count is now equal to iteration count on all benchmarks, useful for users with long-running macro benchmarks and stress tests.

#### 0.2.1 March 22 2016
Fixed issue with NuGet logos and concurrency settings - we now still keep the process priority set to high, as we did before.

#### 0.2.0 March 22 2016
Major changes in NBench v0.2, beginning with our new logo: ![NBench logo](https://github.com/petabridge/NBench/blob/dev/images/NBench_logo_square_140.png)

First, we've added an [extensible plugin API for capturing third-party metrics not natively provided by NBench](https://github.com/petabridge/NBench/pull/86). We will be providing more detailed documentation for this in a later release.

**NBench.PerformanceCounters**
The first example of this can be found in NBench.PerformanceCounters, a brand new NuGet package that allows you to instrument any arbitrary Windows `PerformanceCounter` on any of your tests.

    PS> Install-Package NBench.PerformanceCounters

This package introduces the three following attributes you can use on your benchmarks and performance tests:

* `PerformanceCounterMeasurementAttribute` - measures any available performance counter.
* `PerformanceCounterThroughputAssertion` - asserts a performance counter's *per-second* value.
* `PerformanceCounterTotalAssertion` - asserts a performance counter's *total* value.

**TimingMeasurement and ElapsedTimeAssertion**
Somewhat related to traditional `CounterMeasurement`s, we've added two new attributes which allow you to measure and assert against the total amount of elapsed time it took to run a particular block of code.

* `TimingMeasurementAttribute` - reports on the elapsed time a single run of a benchmark took in milliseconds. Designed to work with `RunMode.Iterations` benchmarks.
* `ElapsedTimeAssertionAttribute` - performs a bounds-checking assertion on amount of time it took to run a particular benchmark. Designed to work with `RunMode.Iterations` benchmarks.

These are now available as part of the core NBench package.

**Additional NBench.Runner Options**
NBench.Runner now supports a new flag argument, `concurrent=true|false`

```
NBench.Runner.exe [assembly names] [output-directory={dir-path}] [configuration={file-path}] [include=MyTest*.Perf*,Other*Spec] [exclude=*Long*] [concurrent={true|false}]
```

**concurrent=true|false** - disables thread priority and processor affinity operations for all benchmarks. Used only when running multi-threaded benchmarks. Set to `false` (single-threaded) by default.

#### 0.1.6 February 15 2016
Includes following features:
* [Support for config files](https://github.com/petabridge/NBench/issues/70)

#### 0.1.5 December 10 2015
Reverted changes to 0.1.4 due to memory profiling issues.

#### 0.1.4 December 10 2015
Resolves the following issue from 0.1.4:
* [Need to ship NBench.Runner.exe.config as part of NuGet.exe](https://github.com/petabridge/NBench/issues/54)

#### 0.1.3 December 8 2015
Resolves the following two issues from 0.1.3:

* [`ReflectionDiscovery` doesn't discover inherited tests on child classes](https://github.com/petabridge/NBench/issues/49)
* [`ReflectionDiscovery` discovers tests on abstract classes](https://github.com/petabridge/NBench/issues/48)

#### 0.1.2 December 8 2015
Bugfix for NBench 0.1.1 where pre-warmup phase of the `Benchmark` could potentially leak memory.

#### 0.1.1 December 5 2015
Bugfix for `NBench.Runner` NuGet package, which [didn't work properly out of the box due to a .dll dependency issue](https://github.com/petabridge/NBench/issues/41). This has been resolved!

#### 0.1.0 December 3 2015
First "production-ready" release of NBench.

Please see our detailed [NBench README and FAQ](https://github.com/petabridge/nbench) for instructions and documentation!

To use NBench, install the NBench package from NuGet:

```
PS> Install-Package NBench
```

And then create a POCO class with a default constructor and some methods, like this:

```csharp
using NBench.Util;
using NBench;

/// <summary>
/// Test to see if we can achieve max throughput on a <see cref="AtomicCounter"/>
/// </summary>
public class CounterPerfSpecs
{
    private Counter _counter;

    [PerfSetup]
    public void Setup(BenchmarkContext context)
    {
        _counter = context.GetCounter("TestCounter");
    }

    [PerfBenchmark(Description = "Test to ensure that a minimal throughput test can be rapidly executed.", 
        NumberOfIterations = 3, RunMode = RunMode.Throughput, 
        RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
    [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, 10000000.0d)]
    [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, ByteConstants.ThirtyTwoKb)]
    [GcTotalAssertion(GcMetric.TotalCollections, GcGeneration.Gen2, MustBe.ExactlyEqualTo, 0.0d)]
    public void Benchmark()
    {
        _counter.Increment();
    }

    [PerfCleanup]
    public void Cleanup(){
        // does nothing
    }
}
```

After defining some NBench `PerfBenchmark` methods and declaring some measurements, you can run your benchmark by downloading the `NBench.Runner.exe` via NuGet.

```
PS> Install-Package NBench.Runner
PS> .\packages\NBench.Runner\NBench.Runner.exe .\src\bin\Debug\MyPerfTests.dll output-directory="C:\Perf"
```

And this command will run your `PerfBenchmark` and write output [that looks like this](https://gist.github.com/Aaronontheweb/8e0bfa2cccc63f5bd8bf) to a markdown file in the `output-directory`.

#### 0.0.2 November 16 2015
First bleeding-edge, alpha release of NBench.

To write an NBench test, use the following syntax:

```csharp
public class SimpleCounterBenchmark
    {
        public const string CounterName = "DumbCounter";
        private Counter _counter;

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _counter = context.GetCounter(CounterName);
        }

        /// <summary>
        /// Run 3 tests, 1 second long each
        /// </summary>
        [PerformanceBenchmark(Description = "Simple iteration collection test", RunMode = RunType.Iterations, TestMode = TestType.Test, RunTimeMilliseconds = 1000, NumberOfIterations = 30)]
        [CounterMeasurement(CounterName)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
        [GcTotalAssertion(GcMetric.TotalCollections, GcGeneration.Gen2, MustBe.ExactlyEqualTo, 0d)]
        public void Run()
        {
            _counter.Increment();
        }

        [PerfCleanup]
        public void CleanUp(BenchmarkContext context)
        {
            //no-op
        }
    }
```
#### 0.0.1 November 3 2015
In development
