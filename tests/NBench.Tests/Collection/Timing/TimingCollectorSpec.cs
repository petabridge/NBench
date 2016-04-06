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
        [Theory]
        [InlineData(100L, 200L)]
        [InlineData(100L, 150L)]
        [InlineData(100L, 120L)]
        [InlineData(200L, 250L)]
        public void TimingCollector_should_report_time_deltas_accurately(long waitTime, long maxAllowedTime)
        {
            var timingCollector = new TimingCollector();
            var initial = timingCollector.Collect();
            Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();
            var next = timingCollector.Collect();
            var delta = next - initial;
            Assert.True(delta < maxAllowedTime, $"Expected a time between {waitTime} ms and {maxAllowedTime} ms - got {delta} ms instead.");
        }
    }
}

