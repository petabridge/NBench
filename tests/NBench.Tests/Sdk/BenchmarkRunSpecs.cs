// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NBench.Collection;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Tracing;
using Xunit;

namespace NBench.Tests.Sdk
{
    public class BenchmarkRunSpecs
    {
        /// <summary>
        ///     When we dispose a <see cref="BenchmarkRun" />, this should also dispose
        ///     all of the <see cref="MeasureBucket" />s and <see cref="MetricCollector" />s.
        /// </summary>
        [Fact]
        public void ShouldDisposeAllCounters()
        {
            var testCollector1 = new TestMetricCollector(new CounterMetricName("foo"), "bar");
            var measureBucket1 = new MeasureBucket(testCollector1);

            var testCollector2 = new TestMetricCollector(new CounterMetricName("foo"), "bar");
            var measureBucket2 = new MeasureBucket(testCollector2);
            var benchmarkRun = new BenchmarkRun(new List<MeasureBucket>(new[] {measureBucket1, measureBucket2}),
                new List<Counter>(), NoOpBenchmarkTrace.Instance);

            Assert.False(testCollector1.WasDisposed);
            Assert.False(testCollector2.WasDisposed);

            benchmarkRun.Dispose();

            Assert.True(testCollector1.WasDisposed);
            Assert.True(testCollector2.WasDisposed);
        }
    }
}

