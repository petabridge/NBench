// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Tests.End2End.SampleBenchmarks
{
    public class CounterThroughputBenchmark
    {
        public const string CounterName = "ThroughputBenchmark";
        private Counter _counter;

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _counter = context.GetCounter(CounterName);
        }

        /// <summary>
        /// Run 3 tests, 1 second long each
        /// </summary>
        [PerfBenchmark(Description = "Counter iteration speed test", RunMode = RunMode.Throughput, TestMode = TestMode.Test, RunTimeMilliseconds = 1000, NumberOfIterations = 3)]
        [CounterThroughputAssertion(CounterName, MustBe.GreaterThan, 1000000.0d)]
        public void Run()
        {
            _counter.Increment();
        }
    }
}

