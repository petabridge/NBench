// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using NBench.Metrics;

namespace NBench.Reporting
{
    /// <summary>
    /// All of the compiled values from one <see cref="BenchmarkRun"/> for a given <see cref="MeasureBucket"/>
    /// </summary>
    public struct MetricRunReport
    {
        public MetricRunReport(MetricName name, string unit, double metricReading, long ticks)
        {
            Contract.Requires(name != null);
            Contract.Requires(!string.IsNullOrEmpty(unit));
            Ticks = Math.Max(ticks, 1);
            Name = name;
            Unit = unit;
            MetricValue = metricReading;
            ElapsedNanos = Ticks/(double) Stopwatch.Frequency*1000000000;
            ElapsedSeconds = ElapsedNanos/1000000000;
            NanosPerMetricValue = ElapsedNanos/Math.Max(MetricValue,1);
            MetricValuePerSecond = MetricValue/ElapsedSeconds;
        }

        /// <summary>
        /// The name of the metric
        /// </summary>
        public MetricName Name { get; private set; }

        /// <summary>
        /// The unit of measure for the metric
        /// </summary>
        public string Unit { get; private set; }

        public long Ticks { get; private set; }

        /// <summary>
        /// The raw values of the <see cref="MeasureBucket"/>
        /// </summary>
        public double MetricValue { get; private set; }

        public double MetricValuePerSecond { get; private set; }

        public double NanosPerMetricValue { get; private set; }

        public double ElapsedNanos { get; private set; }

        public double ElapsedSeconds { get; private set; }
    }
}

