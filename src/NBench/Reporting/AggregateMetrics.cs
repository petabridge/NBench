// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Metrics;
using NBench.Sdk;

namespace NBench.Reporting
{
    /// <summary>
    /// Aggregated metrics accumulated for a single metric across an entire <see cref="Benchmark"/>
    /// </summary>
    public struct AggregateMetrics
    {
        private static IReadOnlyList<MetricRunReport> GetSafeRuns(MetricName name, string unit)
        {
            return new[] {new MetricRunReport(name, unit, MetricRunReport.SafeRawValues)};
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public AggregateMetrics(MetricName name, string unit, IReadOnlyList<MetricRunReport> runs)
        {
            Contract.Requires(runs != null);
            Contract.Requires(name != null);
            Contract.Requires(!string.IsNullOrEmpty(unit));
            Name = name;
            Unit = unit;
            Runs = runs == null || runs.Count == 0 ? GetSafeRuns(name, unit) : runs;

            // Maxes
            Maxes = new BenchmarkStat(runs.Select(x => x.Stats.Max));
            PerSecondMaxes = new BenchmarkStat(runs.Select(x => x.PerSecondStats.Max));

            // Mins
            Mins = new BenchmarkStat(runs.Select(x => x.Stats.Min));
            PerSecondMins = new BenchmarkStat(runs.Select(x => x.PerSecondStats.Min));

            // Averages
            Averages = new BenchmarkStat(runs.Select(x => x.Stats.Average));
            PerSecondAverages = new BenchmarkStat(runs.Select(x => x.PerSecondStats.Average));

            // Sums
            Sums = new BenchmarkStat(runs.Select(x => x.Stats.Sum));
            PerSecondSums = new BenchmarkStat(runs.Select(x => x.PerSecondStats.Sum));

            // StdDev
            StandardDeviations = new BenchmarkStat(runs.Select(x => x.Stats.StandardDeviation));
            PerSecondStandardDeviations = new BenchmarkStat(runs.Select(x => x.PerSecondStats.StandardDeviation));
    
        }

        /// <summary>
        /// The name of the metric
        /// </summary>
        public MetricName Name { get; private set; }

        /// <summary>
        /// The unit of measure for the metric
        /// </summary>
        public string Unit { get; private set; }

        public IReadOnlyList<MetricRunReport> Runs { get; private set; }

        public BenchmarkStat Maxes { get; private set; }

        public BenchmarkStat PerSecondMaxes { get; private set; }

        public BenchmarkStat Mins { get; private set; }

        public BenchmarkStat PerSecondMins { get; private set; }

        public BenchmarkStat Averages { get; private set; }

        public BenchmarkStat PerSecondAverages { get; private set; }

        public BenchmarkStat Sums { get; private set; }

        public BenchmarkStat PerSecondSums { get; private set; }
        public BenchmarkStat StandardDeviations { get; set; }

        public BenchmarkStat PerSecondStandardDeviations { get; set; }
    }
}

