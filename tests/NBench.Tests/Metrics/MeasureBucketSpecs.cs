// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using NBench.Metrics;
using NBench.Util;
using Xunit;

namespace NBench.Tests.Metrics
{
    public class MeasureBucketSpecs
    {
        [Fact]
        public void MeasureBucketShouldDisposeCollector()
        {
            var testCollector = new TestMetricCollector(new CounterMetricName("foo"), "bar");
            var measureBucket = new MeasureBucket(testCollector);
            Assert.False(measureBucket.WasDisposed);
            Assert.False(testCollector.WasDisposed);
            measureBucket.Dispose();
            Assert.True(measureBucket.WasDisposed);
            Assert.True(testCollector.WasDisposed);
        }

        [Theory]
        [InlineData(new[] {0L, 10L, 20L, 30L, 40L}, new[] {1000L, 1001L, 1003L, 1010L, 1012L})]
        [InlineData(new long[] {}, new long[] {})] // no collections
        public void MeasureBucketShouldProduceReport(long[] msValues, long[] counterValues)
        {
            var testCollector = new TestMetricCollector(new CounterMetricName("foo"), "bar");
            var measureBucket = new MeasureBucket(testCollector);
            var length = msValues.Length;
            var timeSpans = msValues.Select(x => TimeSpan.FromMilliseconds(x).Ticks).ToList();
            for (var i = 0; i < length; i++)
            {
                testCollector.CollectorValue = counterValues[i];
                measureBucket.Collect(timeSpans[i]);
            }

            var delta = counterValues.LastOrDefault() - counterValues.FirstOrDefault();
            var report = measureBucket.ToReport();
            Assert.Equal(delta, report.MetricValue);
        }
    }
}

