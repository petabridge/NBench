// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NBench.Metrics;
using NBench.Metrics.Memory;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.Collection.Memory
{
    /// <summary>
    /// Allocator responsible for choosing the appropriate memory collector implementation,
    /// depending on user settings and <see cref="SysInfo"/>.
    /// </summary>
    public class TotalMemorySelector : MetricsCollectorSelector
    {
        public MemoryMetricName MemoryMetricName => (MemoryMetricName)Name;

        public TotalMemorySelector() : this(MetricNames.TotalMemoryAllocated, SysInfo.Instance)
        {
        }

        public TotalMemorySelector(MemoryMetricName name, SysInfo systemInfo) : base(name, systemInfo)
        {
        }

        public override MetricCollector Create(RunMode runMode, WarmupData warmup, IBenchmarkSetting setting)
        {

            //if (warmup.ElapsedTicks <= BenchmarkConstants.SamplingPrecisionTicks)
            return new GcTotalMemoryCollector(MemoryMetricName);
            //return new[] {new PerformanceCounterTotalMemoryCollector(MemoryMetricName)};
        }
    }
}

