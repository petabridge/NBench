using NBench.Util;

namespace NBench.Tests.Performance
{
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

        [PerfBenchmark(Description = "Test to ensure that a minimal throughput test can be rapidly executed.", NumberOfIterations = 3, RunMode = RunMode.Throughput, RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, 10000000.0d)]
        public void Benchmark()
        {
            _counter.Increment();
        }
    }
}
