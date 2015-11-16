// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NBench.Collection;
using NBench.Metrics;
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
            var measureBucket1 = new MeasureBucket(testCollector1, 0);

            var testCollector2 = new TestMetricCollector(new CounterMetricName("foo"), "bar");
            var measureBucket2 = new MeasureBucket(testCollector2, 0);
            var benchmarkRun = new BenchmarkRun(new List<MeasureBucket>(new[] {measureBucket1, measureBucket2}),
                new List<Counter>());

            Assert.False(testCollector1.WasDisposed);
            Assert.False(testCollector2.WasDisposed);

            benchmarkRun.Dispose();

            Assert.True(testCollector1.WasDisposed);
            Assert.True(testCollector2.WasDisposed);
        }

        [Theory]
        [InlineData(new[] { 0L, 10L, 20L, 30L, 40L }, new[] { 1000L, 1001L, 1003L, 1010L, 1012L }, new[] { 0L, 1L, 2L, 3L, 4L })]
        public void ShouldCollectMetricsOnAllCounters(long[] msValues, long[] counterValues1, long[] counterValues2)
        {
            var testCollector1 = new TestMetricCollector(new CounterMetricName("foo"), "bar");
            var measureBucket1 = new MeasureBucket(testCollector1, 0);

            var testCollector2 = new TestMetricCollector(new CounterMetricName("foo"), "bar");
            var measureBucket2 = new MeasureBucket(testCollector2, 0);
            var benchmarkRun = new BenchmarkRun(new List<MeasureBucket>(new[] { measureBucket1, measureBucket2 }),
                new List<Counter>());

            var length = msValues.Length;
            var timeSpans = msValues.Select(x => TimeSpan.FromMilliseconds(x)).ToList();
            for (var i = 0; i < length; i++)
            {
                testCollector1.CollectorValue = counterValues1[i];
                testCollector2.CollectorValue = counterValues2[i];
                benchmarkRun.Sample(timeSpans[i]);
            }

            Assert.Equal(counterValues1, benchmarkRun.Measures[0].RawValues.Values.Select(x => (long)x));
            Assert.Equal(timeSpans, benchmarkRun.Measures[0].RawValues.Keys.ToArray());
            Assert.Equal(counterValues2, benchmarkRun.Measures[1].RawValues.Values.Select(x => (long)x));
            Assert.Equal(timeSpans, benchmarkRun.Measures[1].RawValues.Keys.ToArray());
        }
    }
}

