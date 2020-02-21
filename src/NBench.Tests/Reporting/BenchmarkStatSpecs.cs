﻿// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Reporting;
using Xunit;

namespace NBench.Tests.Reporting
{
    public class BenchmarkStatSpecs
    {
        [Theory]
        [InlineData(new [] { 0L }, 0.0, 0.0, 0.0, 0.0)]
        [InlineData(new long[] {}, 0.0, 0.0, 0.0, 0.0)]
        [InlineData(new[] { 1L, 2L, 3L }, 3.0, 1.0, 2.0, 1.0)]
        public void ShouldComputeBenchmarkStat(long[] values, double max, double min, double average, double stdDev)
        {
            var benchmarkStat = new BenchmarkStat(values);
            Assert.Equal(max, benchmarkStat.Max);
            Assert.Equal(min, benchmarkStat.Min);
            Assert.Equal(average, benchmarkStat.Average);
            Assert.Equal(stdDev, benchmarkStat.StandardDeviation);
        }
    }
}

