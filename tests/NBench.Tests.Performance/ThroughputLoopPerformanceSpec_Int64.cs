namespace NBench.Tests.Performance
{
    public class ThroughputLoopPerformanceSpec_Int64
    {
        /*
          
         */
        private Counter _counter;

        // roughly 74,000,000 operations per second using this while loop
        private const long EsimatedOperationsPerSecond = 37500000L;

        private const int IterationCount = 11;
        private const int OuterOperations = 16;
        private const long InnerOperation = EsimatedOperationsPerSecond / OuterOperations;
        private const TestMode Mode = TestMode.Measurement;
        private const double AcceptableMinValue = 6*10000000.0d; // million op / s

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _counter = context.GetCounter("TestCounter");
        }

        [PerfBenchmark(Description = "Measure the performance of the current While...loop design of the throughput benchmarker",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000, TestMode = Mode)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, AcceptableMinValue)]
        public void Control(BenchmarkContext context)
        {
            var runCount = EsimatedOperationsPerSecond;
            while (true)
            {
                _counter.Increment();
                if (runCount-- == 0)
                    break;
            }
        }

        [PerfBenchmark(Description = "Measure the performance of a for...loop implementation of the same",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000, TestMode = Mode)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, AcceptableMinValue)]
        public void ForLoop(BenchmarkContext context)
        {
            for (var i = EsimatedOperationsPerSecond; i != 0; i--)
            {
                _counter.Increment();
            }
        }

        [PerfBenchmark(Description = "Measure the performance of a for...loop implementation of the same",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000, TestMode = Mode)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, AcceptableMinValue)]
        public void ForLoopInfix(BenchmarkContext context)
        {
            for (var i = EsimatedOperationsPerSecond; i != 0;)
            {
                _counter.Increment();
                --i;
            }
        }

        [PerfBenchmark(Description = "Measure the performance of a for...loop implementation of the same",
            NumberOfIterations = IterationCount, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000, TestMode = Mode)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, AcceptableMinValue)]
        public void ForLoopNested(BenchmarkContext context)
        {

            for (var j = OuterOperations; j != 0; j--)
            {
                for (var i = InnerOperation; i != 0;)
                {
                    _counter.Increment();
                    --i;
                }
            }
        }
    }
}
