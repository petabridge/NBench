// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using NBench.Metrics.Counters;
using NBench.Util;
using Xunit;

namespace NBench.Tests
{
    public class CounterSpecs
    {
        [Fact]
        public void ShouldExplicitlyDecrementCounter()
        {
            var counter = new Counter(new AtomicCounter(), new CounterMetricName("foo"));
            counter.Increment();
            counter.Increment();
            counter.Decrement(2);
            counter.Current.Should().Be(0);
        }

        [Fact]
        public void ShouldExplicitlyIncrementCounter()
        {
            var counter = new Counter(new AtomicCounter(), new CounterMetricName("foo"));
            counter.Increment(10);
            counter.Current.Should().Be(10);
        }
    }
}

