// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Metrics;
using NBench.Util;

namespace NBench.Collection.Counters
{
    /// <summary>
    ///     A simple concurrent counter used for user-defined metrics
    /// </summary>
    public class CounterMetricCollector : MetricCollector
    {
        private readonly AtomicCounter _counter;

        public CounterMetricCollector(MetricName name, AtomicCounter counter)
            : this(name, MetricNames.CounterUnits, counter)
        {
        }

        public CounterMetricCollector(MetricName name, string unitName, AtomicCounter counter) : base(name, unitName)
        {
            _counter = counter;
        }

        public override double Collect()
        {
            return _counter.Current;
        }
    }
}

