// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using NBench.Metrics;
using NBench.Metrics.Timing;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.Collection.Timing
{
    /// <summary>
    /// <see cref="MetricsCollectorSelector"/> implementation for <see cref="TimingMeasurementAttribute"/> 
    /// and <see cref="ElapsedTimeAssertionAttribute"/>
    /// </summary>
    public sealed class TimingSelector : MetricsCollectorSelector
    {
        public TimingSelector() : this(TimingMetricName.Default) { }

        public TimingSelector(MetricName name) : base(name)
        {
        }

        public TimingSelector(MetricName name, SysInfo systemInfo) : base(name, systemInfo)
        {
        }

        public override MetricCollector Create(RunMode runMode, WarmupData warmup, IBenchmarkSetting setting)
        {
            var timingSetting = setting as TimingBenchmarkSetting;
            Contract.Assert(timingSetting != null);

            return new TimingCollector(timingSetting.TimingMetricName);
        }
    }
}

