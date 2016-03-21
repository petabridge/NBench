// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using NBench.Collection;
using NBench.Metrics;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    ///     A <see cref="MetricCollector" /> implementation that uses a <see cref="PerformanceCounter" />
    ///     internally to record various system metrics.
    /// 
    /// Captures the RAW VALUE from performance counters.
    /// </summary>
    public class PerformanceCounterRawValueCollector : MetricCollector
    {
        protected IPerformanceCounterProxy Counter;

        public PerformanceCounterRawValueCollector(MetricName name, string unitName, IPerformanceCounterProxy counter,
            bool disposesCounter) : base(name, unitName)
        {
            Counter = counter;
            DisposesCounter = disposesCounter;
        }

        public bool DisposesCounter { get; }

        public override long Collect()
        {
            try
            {
                return Counter.Collect();
            }
            catch (Exception ex)
            {
                throw new NBenchException($"Failed to get value for PerformanceCounter {Name.ToHumanFriendlyString()} due to underlying error.", ex);
            }
        }

        protected override void DisposeInternal()
        {
            if (DisposesCounter)
            {
                Counter.Dispose();
            }
            Counter = null;
        }
    }
}

