// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Tests.End2End.SampleBenchmarks
{
    public class GcAllGenerationsAssertionBenchmark
    {
        [PerfBenchmark(Description = "Verifier for GC.AllGenerations assertions", RunMode = RunMode.Iterations, TestMode = TestMode.Test,
            RunTimeMilliseconds = 1000, NumberOfIterations = 30)]
        [GcThroughputAssertion(GcMetric.TotalCollections, GcGeneration.AllGc, MustBe.ExactlyEqualTo, 0.0d)]
        public void ShouldNotGc()
        {
            // do nothing
        }
    }
}

