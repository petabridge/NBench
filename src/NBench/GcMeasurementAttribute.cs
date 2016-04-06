// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
namespace NBench
{

    /// <summary>
    /// Specifies the CLR Garbage Collection generation to track during a <see cref="PerfBenchmarkAttribute"/>
    /// </summary>
    public enum GcGeneration
    {
        Gen0 = 0,
        Gen1 = 1,
        Gen2 = 2,
        AllGc = -1,
    }

    /// <summary>
    /// Specifies the CLR garbage collection metric we want to track, usually in combination with a <see cref="GcGeneration"/>.
    /// </summary>
    public enum GcMetric
    {
        /// <summary>
        /// Total number of per-<see cref="GcGeneration"/> collections
        /// </summary>
        TotalCollections,
    }

    /// <summary>
    /// Issues a command to NBench to monitor system GC metrics that occur during the <see cref="PerfBenchmarkAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class GcMeasurementAttribute : MeasurementAttribute
    {
        public GcMeasurementAttribute(GcMetric metric, GcGeneration generation)
        {
            Metric = metric;
            Generation = generation;
        }

        /// <summary>
        /// The GC metric we're going to collect during the benchmark
        /// </summary>
        public GcMetric Metric { get; }

        /// <summary>
        /// The GC generation for which we'll observe <see cref="Metric"/>.
        /// </summary>
        public GcGeneration Generation { get; }
    }

    /// <summary>
    /// Performs an assertion against counters collected over the course of a benchmark.
    /// 
    /// This asserts the NUMBER OF OPERATIONS / SECOND values averaged over all runs of a benchmark.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class GcThroughputAssertionAttribute : GcMeasurementAttribute
    {
        public GcThroughputAssertionAttribute(GcMetric metric, GcGeneration generations,
            MustBe condition, double averageOperationsPerSecond) : base(metric, generations)
        {
            Condition = condition;
            AverageOperationsPerSecond = averageOperationsPerSecond;
        }

        /// <summary>
        /// The test we're going to perform against the collected value of <see cref="GcMeasurementAttribute.Metric"/>
        /// and <see cref="AverageOperationsPerSecond"/>.
        /// </summary>
        public MustBe Condition { get; }

        /// <summary>
        /// The value that will be compared against the collected metric for <see cref="GcMeasurementAttribute.Metric"/>.
        /// </summary>
        public double AverageOperationsPerSecond { get; }

        /// <summary>
        /// Used only on <see cref="MustBe.Between"/> comparisons. This is the upper bound of that comparison
        /// and <see cref="AverageOperationsPerSecond"/> is the lower bound.
        /// </summary>
        public double? MaxAverageOperationsPerSecond { get; set; }
    }

    /// <summary>
    /// Performs an assertion against counters collected over the course of a benchmark.
    /// 
    /// This asserts the TOTAL AVERAGE NUMBER OF OPERATIONS values averaged over all runs of a benchmark.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class GcTotalAssertionAttribute : GcMeasurementAttribute
    {
        public GcTotalAssertionAttribute(GcMetric metric, GcGeneration generations, 
            MustBe condition, double averageOperationsTotal) : base(metric, generations)
        {
            Condition = condition;
            AverageOperationsTotal = averageOperationsTotal;
        }

        /// <summary>
        /// The test we're going to perform against the collected value of <see cref="GcMeasurementAttribute.Metric"/>
        /// and <see cref="AverageOperationsTotal"/>.
        /// </summary>
        public MustBe Condition { get; }

        /// <summary>
        /// The value that will be compared against the collected metric for <see cref="GcMeasurementAttribute.Metric"/>.
        /// </summary>
        public double AverageOperationsTotal { get; }

        /// <summary>
        /// Used only on <see cref="MustBe.Between"/> comparisons. This is the upper bound of that comparison
        /// and <see cref="AverageOperationsTotal"/> is the lower bound.
        /// </summary>
        public double? MaxAverageOperationsTotal { get; set; }
    }
}

