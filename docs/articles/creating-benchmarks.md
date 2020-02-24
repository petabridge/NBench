# Creating Benchmarks and Performance Tests

Here are the different options for creating a `PerfBenchmark`:

* `Description` - optional. Used to describe the purpose of a particular benchmark.
* `RunMode` - sets the run mode for this. Possible options are `RunMode.ThroughPut` and `RunMode.Iteration`.
* `TestMode` - sets the test mode for this benchmark. Possible options are `TestMode.Measurement` and `TestMode.Test`. More on what those options mean in a moment.
* `NumberOfIterations` - determines how many times this benchmark will be run. All final benchmark statistics are reported as an aggregate across all iterations.
* `RunTimeMilliseconds` - for `RunMode.ThroughPut`, this indicates how long we'll attempt to run a test for in order to measure the metric per second values.
* `SkipWarmups` - disables "warmup" iterations that are used to perform cache warming on the CPU. Disabling warmups is often used in long-running iteration tests.

You can declare a `PerfBenchmark` attribute on multiple methods within a single POCO class and each one will be run as its own independent benchmark.

A `PerfBenchmark`, `PerfSetup`, or `PerfCleanup` method can either take no arguments, or it can take an `NBench.BenchmarkContext` object. 

#### Benchmark modes
There are two important modes that you can use in the design of your benchmarks - the `RunMode` and the `TestMode`.

`RunMode` indicates how the benchmark will be run. `RunMode.Throughput` is designed for *very* small benchmarks, like single-line methods, and is meant for scenarios where you really need to measure the throughput of a given operation. 

> [!NOTE]
> During a `Throughput` benchmark the `Benchmark` the `PerfBenchmark` method will be invoked continuously until the estimated time (determined by estimated time gathered during the *Warmup* phase) has elapsed. `PerfSetup` and `PerfCleanup` methods will not be called again until after the throughput test is complete, in other words.

Of course you can also measure things like Garbage Collection and memory allocation too, but those are typically more interesting inside `RunMode.Iteration` tests. `RunMode.Iteration` is designed for running more complex blocks of code where you want to profile things like memory allocation or garbage collection and are less concerned with measuring the throughput of a particular block of code. 

`TestMode` indicates how NBench will evaluate the data at the end of the benchmark. If set to `TestMode.Measure`, NBench will simply report the measurements of all collected values and not perform any unit test-style assertions on any of them; this includes metrics that are declared with "Assertion" attributes such as `CounterThroughputAssertion` and others.

When `TestMode.Test` is enabled, all available assertions will be checked against the collected data. If any such assertions fail, `NBench.Runner.exe` will return with a failure exit code.

#### Benchmark Lifecycle
During the course of each iteration of a benchmark, NBench follows the following object creation lifecycle:

```
Initialize --> PreWarmup --> Warmup --> Benchmark --> Report
```

The purpose of the *Prewarmup* and *Warmup* phases is a little different depending on your `RunMode`, but the general idea is to pre-JIT all of the code that's about to be benchmarked and to estimate how long the benchmark will take to run.

The *Prewarmup*, *Warmup*, and *Benchmark* phases will all perform the following calls against your POCO class:

```
For each iteration:
Create Object --> PerfSetup --> PerfBenchmark --> PerfCleanup --> Destroy Object
```

