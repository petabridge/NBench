// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;

namespace NBench.Collection
{
    /// <summary>
    /// Holds all of the constant metric names used for built-in metrics
    /// </summary>
    public static class MetricNames
    {
        /// <summary>
        /// Used when measuring total memory (in bytes) allocated across a benchmark
        /// </summary>
        public static readonly MemoryMetricName TotalMemoryAllocated = new MemoryMetricName(MemoryMetric.TotalBytesAllocated);

        /// <summary>
        /// The unit of measure for memory allocations
        /// </summary>
        public const string MemoryAllocatedUnits = "bytes";

        /// <summary>
        /// Used when calculating garbage collections per generation
        /// </summary>
        public static readonly GcMetricName GcCollections = new GcMetricName(GcMetric.TotalCollections, GcGeneration.AllGc);

        /// <summary>
        /// Unit of measure for garbage collections
        /// </summary>
        public const string GcCollectionsUnits = "collections";

        /// <summary>
        /// Prefix used for custom counters
        /// </summary>
        public static readonly CounterMetricName CustomCounter = new CounterMetricName("[CUSTOM]");

        /// <summary>
        /// Name of the unit measured by counters
        /// </summary>
        public const string CounterUnits = "operations";

        /// <summary>
        /// The default unit name when none are specified
        /// </summary>
        public const string DefaultUnitName = "operations";

        public const string TimingUnits = "ms";
    }
}

