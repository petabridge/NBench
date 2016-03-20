// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using NBench.Metrics;
using NBench.Metrics.Memory;

namespace NBench.Collection.Memory
{
    /// <summary>
    ///     Measures the total amount of allocated memory that occurs during a performance test run.
    /// </summary>
    public class GcTotalMemoryCollector : MetricCollector
    {
        public GcTotalMemoryCollector(MemoryMetricName name, string unitName) : base(name, unitName)
        {
        }

        public GcTotalMemoryCollector(MemoryMetricName name) : this(name, MetricNames.MemoryAllocatedUnits)
        {
        }

        public GcTotalMemoryCollector() : this(MetricNames.TotalMemoryAllocated)
        {
        }

        public override double Collect()
        {
            /*
             * We intentionally don't allow the garbage collector to collect anything
             * before reporting the amount of memory used. That would defeat the purpose of 
             * the benchmark.
             */
            return GC.GetTotalMemory(false);
        }
    }
}

