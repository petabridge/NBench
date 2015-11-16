#### 0.0.1 November 3 2015
In development

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