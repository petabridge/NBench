// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBench.Collection;
using NBench.Collection.Counters;
using NBench.Collection.GarbageCollection;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Reporting.Targets;
using NBench.Sdk;
using Xunit;

namespace NBench.Tests.Sdk
{
    public class BenchmarkWarmupSpecs
    {
        private static readonly CounterMetricName CounterName = new CounterMetricName("Test");
        private Counter _counter;
        private ActionBenchmarkInvoker _benchmarkMethods;

        public BenchmarkWarmupSpecs()
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
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(31)]
        [InlineData(1000)]
        public void ShouldExecuteCorrectWarmupCount(int iterationCount)
        {
            var observedWarmupCount = -1; //we have a pre-warmup that always happens no matter what. Need to account for it.
            var assertionOutput = new ActionBenchmarkOutput((report, warmup) =>
            {
                if (warmup)
                {
                    observedWarmupCount++;
                }
            }, results =>
            {
                Assert.Equal(iterationCount, observedWarmupCount);
            });

            var counterBenchmark = new CounterBenchmarkSetting(CounterName.CounterName, AssertionType.Total, Assertion.Empty);

            var settings = new BenchmarkSettings(TestMode.Measurement, RunMode.Iterations, iterationCount, 1000,
               new List<IBenchmarkSetting>() { counterBenchmark }, new Dictionary<MetricName, MetricsCollectorSelector>() { 
                   { counterBenchmark.MetricName, new CounterSelector() } });

            var benchmark = new Benchmark(settings, _benchmarkMethods, assertionOutput);

            benchmark.Run();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(31)]
        [InlineData(1000)]
        public void ShouldSkipWarmupsWhenSpecified(int iterationCount)
        {
            var observedWarmupCount = -1; //we have a pre-warmup that always happens no matter what. Need to account for it.
            var assertionOutput = new ActionBenchmarkOutput((report, warmup) =>
            {
                if (warmup)
                {
                    observedWarmupCount++;
                }
            }, results =>
            {
                Assert.Equal(1, observedWarmupCount);
            });

            var counterBenchmark = new CounterBenchmarkSetting(CounterName.CounterName, AssertionType.Total, Assertion.Empty);

            var settings = new BenchmarkSettings(TestMode.Measurement, RunMode.Iterations, iterationCount, 1000,
               new List<IBenchmarkSetting>() { counterBenchmark }, new Dictionary<MetricName, MetricsCollectorSelector>() {
                   { counterBenchmark.MetricName, new CounterSelector() } }) { SkipWarmups = true};

            var benchmark = new Benchmark(settings, _benchmarkMethods, assertionOutput);

            benchmark.Run();
        }
    }
}

