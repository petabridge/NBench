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
            return new[] {new MetricRunReport(name, unit, 0.0d,0)};
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

            Stats = new BenchmarkStat(runs.Select(x => x.MetricValue));
            PerSecondStats = new BenchmarkStat(runs.Select(x => x.MetricValuePerSecond));
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

        public BenchmarkStat Stats { get; private set; }

        public BenchmarkStat PerSecondStats { get; private set; }
    }
}

