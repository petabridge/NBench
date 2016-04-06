// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace NBench.Tests.Performance
{
    public class SimpleTimingAssertionBenchmark
    {
        [PerfBenchmark(Description = "Spec should take about 100ms to execute", RunMode = RunMode.Iterations, NumberOfIterations = 7)]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 140, MinTimeMilliseconds = 100)]
        public void ShouldTakeRoughly100ms()
        {
            Task.Delay(100).Wait();
        }
    }
}

