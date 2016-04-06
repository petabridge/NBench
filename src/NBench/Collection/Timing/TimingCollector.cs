// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using NBench.Metrics;
using NBench.Metrics.Timing;

namespace NBench.Collection.Timing
{
    public sealed class TimingCollector : MetricCollector
    {
        private static readonly Lazy<Stopwatch> Stopwatch = new Lazy<Stopwatch>(() =>
        {
            var s = new Stopwatch();
            s.Start();
            return s;
        });

        public TimingCollector() : this(TimingMetricName.Default) { }

        public TimingCollector(TimingMetricName name) : this(name, MetricNames.TimingUnits) { }

        public TimingCollector(TimingMetricName name, string unitName) : base(name, unitName)
        {

        }

        /// <summary>
        /// Returns the current time back as milliseconds
        /// </summary>
        public override double Collect()
        {
            return Stopwatch.Value.ElapsedMilliseconds;
        }
    }
}

