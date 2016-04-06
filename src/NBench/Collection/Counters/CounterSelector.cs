// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.Collection.Counters
{
    /// <summary>
    /// <see cref="MetricsCollectorSelector"/> used for creating named <see cref="CounterMetricCollector"/> instances.
    /// </summary>
    public class CounterSelector : MetricsCollectorSelector
    {
        public CounterSelector() : this(MetricNames.CustomCounter, SysInfo.Instance) { }

        public CounterSelector(MetricName name, SysInfo systemInfo) : base(name, systemInfo)
        {
        }

        public override MetricCollector Create(RunMode runMode, WarmupData warmup, IBenchmarkSetting setting)
        {
            Contract.Assert(setting != null);
            Contract.Assert(setting is CreateCounterBenchmarkSetting);
            var createCounter = setting as CreateCounterBenchmarkSetting;

            // ReSharper disable once PossibleNullReferenceException
            // resolved with Code Contracts
            return new CounterMetricCollector(createCounter.BenchmarkSetting.CounterName, createCounter.Counter);
        }
    }
}

