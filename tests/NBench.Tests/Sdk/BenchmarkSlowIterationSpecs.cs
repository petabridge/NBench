// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBench.Collection;
using NBench.Collection.Counters;
using NBench.Collection.Memory;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Reporting.Targets;
using NBench.Sdk;
using Xunit;

namespace NBench.Tests.Sdk
{
    /// <summary>
    ///     Specs designed to test <see cref="Benchmark" />s with <see cref="RunMode.Iterations" /> and SLOW run-times.
    ///     Uses virtually the same execution mode as <see cref="RunMode.Throughput"/>.
    /// </summary>
    public class BenchmarkSlowIterationSpecs
    {
        private static readonly CounterMetricName CounterName = new CounterMetricName("Test");
        private Counter _counter;
        private ActionBenchmarkInvoker _benchmarkMethods;

        public BenchmarkSlowIterationSpecs()
        {
            _benchmarkMethods = new ActionBenchmarkInvoker(GetType().Name, BenchmarkSetupMethod, BenchmarkTestMethod,
                ActionBenchmarkInvoker.NoOp);
        }

        public void BenchmarkSetupMethod(BenchmarkContext context)
        {
            _counter = context.GetCounter(CounterName.CounterName);
        }

        public void BenchmarkTestMethod(BenchmarkContext context)
        {
            _counter.Increment();
            {
                var bytes = new byte[1 << 13];
                bytes = null;
            }
            Thread.Sleep(30);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)] // keep the values small since there's a real delay involved
        public void ShouldComputeMetricsCorrectly(int iterationCount)
        {
            var assertionOutput = new ActionBenchmarkOutput((report, warmup) =>
            {
                if (!warmup)
                {
                    var counterResults = report.Metrics[CounterName];
                    Assert.Equal(1, counterResults.MetricValue);
                }
               
            }, results =>
            {
                var counterResults = results.Data.StatsByMetric[CounterName].Stats.Max;
                Assert.Equal(iterationCount, counterResults);
            });

            var counterBenchmark = new CounterBenchmarkSetting(CounterName.CounterName, AssertionType.Total, Assertion.Empty);
            var gcBenchmark = new GcBenchmarkSetting(GcMetric.TotalCollections, GcGeneration.Gen2, AssertionType.Total,
                Assertion.Empty);
            var memoryBenchmark = new MemoryBenchmarkSetting(MemoryMetric.TotalBytesAllocated, Assertion.Empty);

            var settings = new BenchmarkSettings(TestMode.Measurement, RunMode.Iterations, iterationCount, 0,
               new List<IBenchmarkSetting>() { gcBenchmark, counterBenchmark },
                new Dictionary<MetricName, MetricsCollectorSelector>()
                {
                    { gcBenchmark.MetricName, new GcCollectionsSelector() },
                    { counterBenchmark.MetricName, new CounterSelector() },
                    { memoryBenchmark.MetricName, new TotalMemorySelector() }
                });

            var benchmark = new Benchmark(settings, _benchmarkMethods, assertionOutput);

            benchmark.Run();
        }
    }
}

