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