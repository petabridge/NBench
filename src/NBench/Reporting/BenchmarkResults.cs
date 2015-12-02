// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Metrics;
using NBench.Sdk;

namespace NBench.Reporting
{
    /// <summary>
    ///     The cumulative results for an entire <see cref="Benchmark" />
    /// </summary>
    public class BenchmarkResults
    {
        public BenchmarkResults(string typeName, BenchmarkSettings settings, IReadOnlyList<BenchmarkRunReport> runs)
        {
            Contract.Requires(!string.IsNullOrEmpty(typeName));
            Contract.Requires(runs != null);
            BenchmarkName = typeName;
            Settings = settings;
            Runs = runs;
            StatsByMetric = new Dictionary<MetricName, AggregateMetrics>();
            StatsByMetric = Aggregate(Runs);
            Exceptions = Runs.SelectMany(r => r.Exceptions).ToList();
        }

        /// <summary>
        /// Usually prints out the type name of the spec being run
        /// </summary>
        public string BenchmarkName { get; private set; }

        /// <summary>
        ///     The settings for this <see cref="Benchmark" />
        /// </summary>
        public BenchmarkSettings Settings { get; private set; }

        /// <summary>
        /// The set of <see cref="Exception"/>s that may have occurred during a benchmark.
        /// </summary>
        public IReadOnlyList<Exception> Exceptions { get; private set; }

        /// <summary>
        /// Returns <c>true</c> if any <see cref="Exception"/>s were thrown during this run.
        /// </summary>
        public bool IsFaulted => Exceptions.Count > 0;

        /// <summary>
        /// The list of raw data available for each run of the benchmark
        /// </summary>
        public IReadOnlyList<BenchmarkRunReport> Runs { get; }

        /// <summary>
        ///     Per-metric aggregate statistics
        /// </summary>
        public IReadOnlyDictionary<MetricName, AggregateMetrics> StatsByMetric { get; private set; }

        public static IReadOnlyDictionary<MetricName, AggregateMetrics> Aggregate(IReadOnlyList<BenchmarkRunReport> runs)
        {
            var intermediate = new Dictionary<MetricName, List<MetricRunReport>>();

            foreach (var metric in runs.SelectMany(run => run.Metrics))
            {
                if (!intermediate.ContainsKey(metric.Key))
                    intermediate.Add(metric.Key, new List<MetricRunReport>());
                intermediate[metric.Key].Add(metric.Value);
            }

            return intermediate.ToDictionary(k => k.Key,
                v => new AggregateMetrics(v.Value.First().Name, v.Value.First().Unit, v.Value));
        }
    }
}

