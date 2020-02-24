// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBench.Collection.Timing;
using Xunit;
using Xunit.Sdk;

namespace NBench.Tests.Collection.Timing
{
    public class TimingCollectorSpec
    {
        [Theory(Skip = "Flaky")]
        [InlineData(100L, 200L)]
        [InlineData(100L, 150L)]
        // [InlineData(100L, 120L)] skipped because flakiness (result was ~120+)
        [InlineData(200L, 300L)] // increased from 250 to 300 because flakiness (result was ~250+)
        public void TimingCollector_should_report_time_deltas_accurately(long waitTime, long maxAllowedTime)
        {
            var timingCollector = new TimingCollector();
            var delay = TimeSpan.FromMilliseconds(waitTime);
            var initial = timingCollector.Collect();
            Task.Delay(delay).Wait(delay);
            var next = timingCollector.Collect();
            var delta = next - initial;
            Assert.True(delta < maxAllowedTime, $"Expected a time between {waitTime} ms and {maxAllowedTime} ms - got {delta} ms instead.");
        }
    }
}

