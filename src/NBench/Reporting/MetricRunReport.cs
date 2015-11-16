// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Metrics;
using NBench.Util;

namespace NBench.Reporting
{
    /// <summary>
    /// All of the compiled values from one <see cref="BenchmarkRun"/> for a given <see cref="MeasureBucket"/>
    /// </summary>
    public struct MetricRunReport
    {
        internal static readonly IDictionary<TimeSpan, long> SafeRawValues = new Dictionary<TimeSpan, long>() { { TimeSpan.Zero, 0L } };

        public MetricRunReport(MetricName name, string unit, IDictionary<TimeSpan, long> rawValues)
        {
            Contract.Requires(name != null);
            Contract.Requires(!string.IsNullOrEmpty(unit));
            Contract.Requires(rawValues != null);
            Name = name;
            Unit = unit;
            RawValues = rawValues.Count == 0 ? SafeRawValues : rawValues;
            var deltas = RawValues.DistanceFromStart();
            Stats = new BenchmarkStat(deltas.Values);
            Elapsed = deltas.Keys.Max();
            PerSecondStats = new PerSecondBenchmarkStat(Stats, Elapsed.TotalSeconds);
        }

        /// <summary>
        /// The name of the metric
        /// </summary>
        public MetricName Name { get; private set; }

        /// <summary>
        /// The unit of measure for the metric
        /// </summary>
        public string Unit { get; private set; }

        /// <summary>
        /// The raw values of the <see cref="MeasureBucket"/>
        /// </summary>
        public IDictionary<TimeSpan, long> RawValues { get; }

        /// <summary>
        /// Aggregate statistics for this run.
        /// </summary>
        public BenchmarkStat Stats { get; }

        /// <summary>
        /// The total amount of elapsed time
        /// </summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Total value per second for this stat
        /// </summary>
        public PerSecondBenchmarkStat PerSecondStats { get; private set; }
    }
}

