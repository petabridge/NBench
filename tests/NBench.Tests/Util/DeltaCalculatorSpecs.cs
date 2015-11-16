// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NBench.Metrics;
using Xunit;
using NBench.Util;

namespace NBench.Tests.Util
{
    public class DeltaCalculatorSpecs
    {
        [Theory]
        [InlineData(new[]{ 0L, 0L, 10L, 100L, 20L, 200L}, new[] { 0.0d, 100.0d, 200.0d })]
        [InlineData(new[] { 0L, 121L, 10L, 221L, 20L, 421L }, new[] { 0.0d, 100.0d, 300.0d })]
        public void ShouldCalculateDeltas(long[] timeValuePairs, double[] expectedValues)
        {
            var pairs = timeValuePairs.Zip((time, value) => new MetricMeasurement(new TimeSpan(time), value));
            var queue = new Queue<MetricMeasurement>(pairs);
            var deltas = queue.DistanceFromStart();

            Assert.True(deltas.Values.SequenceEqual(expectedValues));
        }
    }
}

