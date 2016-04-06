// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NBench.Collection;
using NBench.Collection.Memory;
using NBench.Sdk;

namespace NBench.Metrics.Memory
{
    public class MemoryMeasurementConfigurator : MeasurementConfigurator<MemoryMeasurementAttribute>
    {
        public static readonly TotalMemorySelector TotalMemorySelector = new TotalMemorySelector();

        public override MetricName GetName(MemoryMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            return new MemoryMetricName(instance.Metric);
        }

        public override MetricsCollectorSelector GetMetricsProvider(MemoryMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            if(instance.Metric == MemoryMetric.TotalBytesAllocated)
                return TotalMemorySelector;
            throw new NotSupportedException($"MemoryMeasurementConfigurator does not support memory metric {instance.Metric}");
        }

        public override IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(MemoryMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            var assertion = instance as MemoryAssertionAttribute;
            if (assertion != null)
            {
                return new [] {new MemoryBenchmarkSetting(instance.Metric,
                    new Assertion(assertion.Condition, assertion.AverageBytes, assertion.MaxAverageBytes))};
            }
            return new [] {new MemoryBenchmarkSetting(instance.Metric, Assertion.Empty) };
        }
    }
}

