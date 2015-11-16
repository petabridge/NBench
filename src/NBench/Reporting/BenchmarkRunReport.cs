// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NBench.Metrics;

namespace NBench.Reporting
{
    /// <summary>
    ///     Compiled statistics for each <see cref="BenchmarkRun" />
    /// </summary>
    public struct BenchmarkRunReport
    {
        public BenchmarkRunReport(TimeSpan elapsed, IEnumerable<MetricRunReport> metrics)
            : this(elapsed, metrics.ToDictionary(k => k.Name, v => v))
        {
        }

        public BenchmarkRunReport(TimeSpan elapsed, IDictionary<MetricName, MetricRunReport> metrics)
        {
            Elapsed = elapsed;
            Metrics = metrics;
        }

        /// <summary>
        ///     Total amount of elapsed time on this run
        /// </summary>
        public TimeSpan Elapsed { get; private set; }

        /// <summary>
        ///     Key value pair of all metrics, where the key corresponds to the name of the metric
        /// </summary>
        public IDictionary<MetricName, MetricRunReport> Metrics { get; private set; }
    }
}

