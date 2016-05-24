// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NBench.Collection;
using NBench.Collection.Counters;
using NBench.Collection.GarbageCollection;
using NBench.Collection.Memory;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Sdk;
using NBench.Sys;
using NBench.Tracing;
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
                new CounterBenchmarkSetting[] { counterBenchmark}, new Dictionary<MetricName, MetricsCollectorSelector>() { { counterBenchmark.MetricName, new CounterSelector() } });

            var builder = new BenchmarkBuilder(settings);
            var run = builder.NewRun(WarmupData.PreWarmup);

            Assert.Equal(1, run.MeasureCount);
            Assert.Equal(1, run.Counters.Count);
            Assert.True(run.Counters.ContainsKey(counterBenchmark.CounterName));
        }

        [Fact]
        public void Should_build_when_at_least_one_metric_assigned()
        {
            var counterBenchmark = new CounterBenchmarkSetting("Test", AssertionType.Total, Assertion.Empty);
            var gcBenchmark = new GcBenchmarkSetting(GcMetric.TotalCollections, GcGeneration.Gen2, AssertionType.Total,
                Assertion.Empty);
            var memoryBenchmark = new MemoryBenchmarkSetting(MemoryMetric.TotalBytesAllocated, Assertion.Empty);
            var settings = new BenchmarkSettings(TestMode.Measurement, RunMode.Iterations, 10, 1000,
                new List<IBenchmarkSetting>(){gcBenchmark, memoryBenchmark, counterBenchmark }, 
                new Dictionary<MetricName, MetricsCollectorSelector>()
                {
                    { gcBenchmark.MetricName, new GcCollectionsSelector() },
                    { counterBenchmark.MetricName, new CounterSelector() },
                    { memoryBenchmark.MetricName, new TotalMemorySelector() }
                });

            var builder = new BenchmarkBuilder(settings);
            var run = builder.NewRun(WarmupData.PreWarmup);

            Assert.Equal(3, run.MeasureCount);
            Assert.Equal(1, run.Counters.Count);
            Assert.True(run.Counters.ContainsKey(counterBenchmark.CounterName));
        }
    }
}

