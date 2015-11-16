// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Metrics;
using NBench.Sdk;
using NBench.Sys;
using Xunit;

namespace NBench.Tests.Sdk
{
    public class BenchmarkBuilderSpecs
    {
        [Fact]
        public void Should_build_when_exactly_one_metric_assigned()
        {
            var counterBenchmark = new CounterBenchmarkSetting("Test", AssertionType.Total, Assertion.Empty);
            var settings = new BenchmarkSettings(TestMode.Measurement, RunMode.Iterations, 10, 1000,
                new GcBenchmarkSetting[0], new MemoryBenchmarkSetting[0], new CounterBenchmarkSetting[] { counterBenchmark});

            var builder = new BenchmarkBuilder(settings);
            var run = builder.NewRun(WarmupData.Empty);

            Assert.Equal(1, run.MeasureCount);
            Assert.Equal(1, run.Counters.Count);
            Assert.True(run.Counters.ContainsKey(counterBenchmark.CounterName));
        }

        [Fact]
        public void Should_build_when_at_least_one_metric_assigned()
        {
            var counterBenchmark = new CounterBenchmarkSetting("Test", AssertionType.Total, Assertion.Empty);
            var gcBenchmark = new GcBenchmarkSetting(GcMetric.TotalCollections, GcGeneration.AllGc, AssertionType.Total,
                Assertion.Empty);
            var memoryBenchmark = new MemoryBenchmarkSetting(MemoryMetric.TotalBytesAllocated, Assertion.Empty);
            var settings = new BenchmarkSettings(TestMode.Measurement, RunMode.Iterations, 10, 1000,
                new[] {gcBenchmark}, new[] { memoryBenchmark }, new[] { counterBenchmark });

            var builder = new BenchmarkBuilder(settings);
            var run = builder.NewRun(WarmupData.Empty);

            Assert.Equal(2 + (SysInfo.Instance.MaxGcGeneration + 1), run.MeasureCount);
            Assert.Equal(1, run.Counters.Count);
            Assert.True(run.Counters.ContainsKey(counterBenchmark.CounterName));
        }
    }
}

