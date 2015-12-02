// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Tests.Performance
{
    public class IterationModeMeasurementSpec
    {
        private Counter _counter1;
        private Counter _counter2;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            _counter1 = context.GetCounter("Counter1");
            _counter2 = context.GetCounter("Counter2");
        }

        [PerfBenchmark(Description = "Test to ensure that the math following an iteration mode test is accurate",
            NumberOfIterations = 3, RunMode = RunMode.Iterations, RunTimeMilliseconds = 1000, TestMode = TestMode.Test)]
        [CounterTotalAssertion("Counter1", MustBe.GreaterThanOrEqualTo, 1000.0d)]
        [CounterTotalAssertion("Counter1", MustBe.GreaterThan, 500.0d)] // duplicate, to see if multiple assertions against same counter run
        [CounterTotalAssertion("Counter2", MustBe.GreaterThanOrEqualTo, 1000.0d)]
        [CounterMeasurement("Counter2")]
        public void Benchmark()
        {
            for (var i = 0; i < 1000; i++)
            {
                _counter1.Increment();
                _counter2.Increment();
            }
        }
    }
}

