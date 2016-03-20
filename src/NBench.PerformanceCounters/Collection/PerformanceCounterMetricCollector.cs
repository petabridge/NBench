// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NBench.Collection;
using NBench.Metrics;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    ///     A <see cref="MetricCollector" /> implementation that uses a <see cref="PerformanceCounter" />
    ///     internally to record various system metrics.
    /// </summary>
    public class PerformanceCounterMetricCollector : MetricCollector
    {
        private PerformanceCounter _counter;

        public PerformanceCounterMetricCollector(MetricName name, string unitName, PerformanceCounter counter,
            bool disposesCounter) : base(name, unitName)
        {
            _counter = counter;
            DisposesCounter = disposesCounter;
        }

        public bool DisposesCounter { get; }

        public override long Collect()
        {
            return _counter.RawValue;
        }

        protected override void DisposeInternal()
        {
            if (DisposesCounter)
            {
                _counter.Dispose();
            }
            _counter = null;
        }
    }
}

