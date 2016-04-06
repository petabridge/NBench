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
