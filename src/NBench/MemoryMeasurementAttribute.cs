// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench
{
    /// <summary>
    ///     Available memory-related metrics that can be profiled and tested against
    /// </summary>
    public enum MemoryMetric
    {
        /// <summary>
        ///     Measure the total bytes allocated during a benchmark
        /// </summary>
        TotalBytesAllocated
    }

    /// <summary>
    ///     Issues a command to NBench to monitor various memory metrics allocated
    ///     during the <see cref="PerfBenchmarkAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MemoryMeasurementAttribute : MeasurementAttribute
    {
        public MemoryMeasurementAttribute(MemoryMetric metric)
        {
            Metric = metric;
        }

        /// <summary>
        ///     The memory-specific metric we're going to track.
        /// </summary>
        public MemoryMetric Metric { get; }
    }

    /// <summary>
    ///     Performs an assertion against memory counters collected over the course of a benchmark.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MemoryAssertionAttribute : MemoryMeasurementAttribute
    {
        public MemoryAssertionAttribute(MemoryMetric metric, MustBe condition, double averageBytes) : base(metric)
        {
            Condition = condition;
            AverageBytes = averageBytes;
        }

        /// <summary>
        ///     The test we're going to perform against the collected value of <see cref="MemoryMetric" />
        ///     and <see cref="AverageBytes" />.
        /// </summary>
        public MustBe Condition { get; }

        /// <summary>
        ///     The value that will be compared against the collected metric for <see cref="MemoryMetric" />.
        /// </summary>
        public double AverageBytes { get; }

        /// <summary>
        ///     Used only on <see cref="MustBe.Between" /> comparisons. This is the upper bound of that comparison
        ///     and <see cref="AverageBytes" /> is the lower bound.
        /// </summary>
        public double? MaxAverageBytes { get; set; }
    }
}

