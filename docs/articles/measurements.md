# Measurements and Assertions

As of the most recent stable release, NBench allows you to gather the following types of data:

1. Memory allocation - how much memory has the code in your benchmark demanded from the CLR?
1. Garbage collection (GC) - how many collections for each GC generation has the code in your benchmark used?
1. Counters - how many calls were made against a specific counter by your code?

## Memory
There are two types of attributes you can use to instrument a benchmark for memory measurement:

* `MemoryMeasurement` - just measures a pre-defined memory metric without specifying any assertions against the data.
* `MemoryAssertion` - collects data and performs an assertion.

Inside the constructor of each of these attributes you can specify one of a possible number of memory metrics. Right now here is what NBench supports:

* `MemoryMetric.TotalBytesAllocated` - Measure the total bytes allocated during a benchmark.

To collect data for this metric, declare an attribute on your `PerfBenchmark` method like so:

```csharp
class MyMemoryBenchmark{
    [PerfBenchmark(NumberOfIterations = 3, RunMode = RunMode.Iteration, 
        RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
    [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, 
        ByteConstants.ThirtyTwoKb)]
    public void SomeBenchmarkMethod()
    {
        // some code
    }
}
```

In the example above, if the block of code inside `SomeBenchmarkMethod` causes the CLR allocate more than 32kb of memory on average across 3 iterations of this code then the `MemoryAssertion` will write a failure into the report produced by NBench.

> NOTE: The way `MemoryMetric.TotalBytesAllocated` is measured is by taking a *before* and *after* snapshot of total memory as reported by the CLR Garbage Collector. 

Memory is typically allocated in pages, i.e. the OS might allocate 8kb when all you need is 12 bytes, that way it doesn't have to constantly allocate lots of small segments of memory to the process, so **to get best results it's recommended that you write your memory benchmarks such that you allocate a large number of the objects you want to benchmark**. That helps average out the noise produced by this allocation strategy on the part of the OS.

## Garbage Collection
The goal of checking GC metrics is to help eliminate the garbage collector as a source of pauses, slowdowns, and negative impact on throughput in your application. Therefore NBench's GC measurement capabilities are designed to help you track the number of GC collections that occur at each generation as part of your benchmark.

Here are the types of attributes you can use for measuring GC inside your benchmarks:

* `GcMeasurement` - measure the number of collections that occur for the specified GC generation without performing any assertions.
* `GcTotalAssertion` - measure and assert against the total number of collections that occurred at the specific GC generation.
* `GcThroughputAssertion` - measure and assert against the total number of collections **per second** that occurred at the specific GC generation.

Inside the constructor of each of these attributes you can specify the type of GC metric you want to collect. Here's what is currently supported inside NBench:

* `GcMetric.TotalCollections` - the total number of GC collections at the specified generation.

You must also specify one of the following GC generations in the constructor of your attribute, as it indicates for which GC generation you want to apply this measurement:

* `GcGeneration.Gen0` - Gen 0.
* `GcGeneration.Gen1` - Gen 1.
* `GcGeneration.Gen2` - Gen 2. This is usually the one you want to pay close attention to.
* `GcGeneration.AllGc` - all supported generations.

To collection GC data, declare one of the GC measurement attributes on your `PerfBenchmark` method like this:

```csharp
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

In the example above, this spec would fail if any Gen2 garbage collection occurred throughout the 1000ms duration of this benchmark (which, based on the reading of this code, none should.)

> NOTE: NBench will force the .NET garbage collector to perform a full cleanup before the benchmark runs and AFTER it completes. It does not force the garbage collector to run *before* we measure GC collection attempts. So to get the best possible measurement, design your GC benchmarks such that they offer opportunities for the GC to run. Long-running iteration benchmarks that repeat the same code deterministically (i.e. a `for` loop) are one option.

## `BenchmarkContext`, Counters, and Throughput
The role of `BenchmarkContext` is solely for accessing user-defined counters that are declared using any one of the following three attributes:

* `CounterThroughputAssertion` - used to perform an assertion against the number of operations per second measured by this named counter.
* `CounterTotalAssertion` - used to perform an assertion against the TOTAL number of operations measured by this named counter.
* `CounterMeasurement` - used to simply measure and report on a counter without any sort of assertions.

You can declare multiple counter attributes that all measure against the same counter, i.e.

```csharp
public class CounterExample
{
    private Counter _counter;

    [PerfSetup]
    public void Setup(BenchmarkContext context)
    {
        _counter = context.GetCounter("TestCounter");
    }

    // Perfectly valid counter setup
    [PerfBenchmark(NumberOfIterations = 3, RunMode = RunMode.Throughput, 
        RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
    [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, 10000000.0d)]
    [CounterTotalAssertion("TestCounter", MustBe.GreaterThan, 10000000.0d)]
    [CounterMeasurement("TestCounter")]
    public void Benchmark()
    {
        _counter.Increment();
    }
}
```

To gain access to a `Counter` for use inside your benchmark, you will need to pass in the `BenchmarkContext` to either your `PerfSetup` method or your `PerfBenchmark` method, as shown in the example above. From there you can call `context.GetCounter("{YOUR COUNTER NAME}")` and retrieve access to the counter instance being tracked by NBench.

> NOTE: If you call `BenchmarkContext.GetCounter(string counterName)` and `counterName` doesn't match the name of any of your counters declared in your `CounterThroughputAssertion`, `CounterTotalAssertion`, and `CounterMeasurement` attributes then you will receive an `NBenchException`.

### Best Practices for Working with `Counter`s
Here are a few best practices to bear in mind when working with counters:

1. It's always best to store references to your `Counter` instances as fields inside your POCO class and to get those references during a `PerfSetup` call, rather than get references to them on-the-fly inside your benchmark.
1. Adding the `BenchmarkContext` parameter to your `PerfBenchmark` methods will improve throughput slightly, due to a design of NBench's `ReflectionInvoker`.