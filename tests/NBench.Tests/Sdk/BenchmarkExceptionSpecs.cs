// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using NBench.Collection;
using NBench.Collection.Memory;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using Xunit;

namespace NBench.Tests.Sdk
{
    public class BenchmarkExceptionSpecs
    {
        private static readonly GcBenchmarkSetting _singleFaultySetting = new GcBenchmarkSetting(GcMetric.TotalCollections,
            GcGeneration.Gen2, AssertionType.Throughput,
            Assertion.Empty);
        private readonly BenchmarkSettings _faultySettings = new BenchmarkSettings(TestMode.Test, RunMode.Iterations, 4, 1000,
            new List<IBenchmarkSetting>
            {
               _singleFaultySetting
            }, new Dictionary<MetricName, MetricsCollectorSelector>() { { _singleFaultySetting.MetricName, new GcCollectionsSelector() } });

        [Fact]
        public void Should_exit_on_first_iteration_where_Benchmark_throws_Exception()
        {
            var iterationCount = 0;
            var expectedIterationCount = 1;
            bool finalOutputCalled = false;
            IBenchmarkInvoker faultyInvoker = new ActionBenchmarkInvoker("ExceptionThrower",
                context =>
                {
                    
                    throw new Exception("poorly written spec");
                });

            IBenchmarkOutput faultyOutput = new ActionBenchmarkOutput(
                (report, isWarmup) =>
                {
                    iterationCount++; //should run exactly once during pre-warmup
                    Assert.True(report.IsFaulted);
                }, results =>
                {
                    finalOutputCalled = true;
                    Assert.True(results.Data.IsFaulted);
                });

            var benchmark = new Benchmark(_faultySettings, faultyInvoker, faultyOutput);
            benchmark.Run();
            benchmark.Finish();
            Assert.Equal(expectedIterationCount, iterationCount);
            Assert.True(finalOutputCalled);
        }
    }
}

