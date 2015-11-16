// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using NBench.Collection;
using NBench.Collection.Memory;
using Xunit;

namespace NBench.Tests.Collection.Memory
{
    public class PerformanceCounterTotalMemoryCollectorSpecs
    {
        public const double Accuracy = 0.1;

        [Theory(Skip = "PerformanceCounters are totally inaccurate compared to GC for this type of benchmarking")]
        [InlineData(1 << 13, Accuracy)]
        [InlineData(1 << 14, Accuracy)]
        [InlineData(1 << 18, Accuracy)]
        [InlineData(1 << 24, Accuracy)]
        [InlineData(1 << 25, Accuracy)]
        public void PerformanceCounterTotalMemoryCollector_Should_be_Accurate_to_within_10_percent(int bytesAllocated, double accuracy)
        {
            GC.Collect(); //collect everything first
            using (var totalMemoryCollector = new PerformanceCounterTotalMemoryCollector(MetricNames.TotalMemoryAllocated))
            {
                long initialReading = totalMemoryCollector.Collect();
                {
                    var bytes = new byte[bytesAllocated];
                    bytes = null;
                }
                long finalReading = totalMemoryCollector.Collect();
                long delta = finalReading - initialReading;
                Assert.True(finalReading > initialReading, "Should be: finalReading > initialReading");
                double actualDifference = Math.Abs(delta - bytesAllocated) / (double)bytesAllocated;
                Assert.True(actualDifference <= accuracy,
                    $"Expected {delta} to be within {accuracy} of {bytesAllocated} but was {actualDifference}");
            }
        }
    }
}

