// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using NBench.Metrics;

namespace NBench.Collection.Memory
{
    /// <summary>
    ///     Collects the total amount of allocated memory using a <see cref="PerformanceCounter" /> for working set memory.
    /// </summary>
    public class PerformanceCounterTotalMemoryCollector : PerformanceCounterMetricCollector
    {
        public PerformanceCounterTotalMemoryCollector() : this(true)
        {
        }

        public PerformanceCounterTotalMemoryCollector(bool disposesCounter)
            : this(MetricNames.TotalMemoryAllocated, disposesCounter)
        {
        }

        public PerformanceCounterTotalMemoryCollector(MemoryMetricName name) : this(name, true)
        {
        }

        public PerformanceCounterTotalMemoryCollector(MemoryMetricName name, bool disposesCounter)
            : this(name, GetTotalProcessMemoryCounter(), disposesCounter)
        {
        }

        public PerformanceCounterTotalMemoryCollector(PerformanceCounter counter, bool disposesCounter)
            : this(MetricNames.TotalMemoryAllocated, counter, disposesCounter)
        {
        }

        public PerformanceCounterTotalMemoryCollector(MemoryMetricName name, PerformanceCounter counter, bool disposesCounter)
            : this(name, MetricNames.MemoryAllocatedUnits, counter, disposesCounter)
        {
        }

        public PerformanceCounterTotalMemoryCollector(MemoryMetricName name, string unitName, PerformanceCounter counter,
            bool disposesCounter)
            : base(name, unitName, counter, disposesCounter)
        {
        }

        /// <summary>
        ///     Returns the <see cref="PerformanceCounter" /> that corresponds to Total Working Set memory
        ///     for this process.
        /// </summary>
        /// <returns>A new <see cref="PerformanceCounter" /> instance.</returns>
        public static PerformanceCounter GetTotalProcessMemoryCounter()
        {
            return new PerformanceCounter("Process", "Private Bytes", Process.GetCurrentProcess().ProcessName);
        }
    }
}

