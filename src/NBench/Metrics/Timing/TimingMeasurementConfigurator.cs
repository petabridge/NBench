// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NBench.Collection;
using NBench.Collection.Timing;
using NBench.Sdk;

namespace NBench.Metrics.Timing
{
    public class TimingMeasurementConfigurator : MeasurementConfigurator<TimingMeasurementAttribute>
    {
        public static readonly TimingSelector Selector = new TimingSelector();

        public override MetricName GetName(TimingMeasurementAttribute instance)
        {
            // right now, there is only one type of TimingMetricName
            return TimingMetricName.Default;
        }

        public override MetricsCollectorSelector GetMetricsProvider(TimingMeasurementAttribute instance)
        {
            return Selector;
        }

        public override IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(TimingMeasurementAttribute instance)
        {
            var elapsedTimeAssertionAttribute = instance as ElapsedTimeAssertionAttribute;
            var settings = new List<TimingBenchmarkSetting>();
            var metricName = (TimingMetricName) GetName(instance);
            if (elapsedTimeAssertionAttribute == null)
            {
                settings.Add(new TimingBenchmarkSetting(metricName, Assertion.Empty));
                return settings;
            }

            Contract.Assert(elapsedTimeAssertionAttribute.MaxTimeMilliseconds > 0);
            if (elapsedTimeAssertionAttribute.MinTimeMilliseconds == 0)
            {
                var setting = new TimingBenchmarkSetting(metricName,
                    new Assertion(MustBe.LessThanOrEqualTo, elapsedTimeAssertionAttribute.MaxTimeMilliseconds, null));
                settings.Add(setting);
            }
            else
            {
                Contract.Assert(elapsedTimeAssertionAttribute.MinTimeMilliseconds >= 0); // no negative values allowed
                var setting = new TimingBenchmarkSetting(metricName,
                  new Assertion(MustBe.Between, elapsedTimeAssertionAttribute.MinTimeMilliseconds, elapsedTimeAssertionAttribute.MaxTimeMilliseconds));
                settings.Add(setting);
            }

            return settings;
        }
    }
}

