// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NBench.Collection;
using NBench.Collection.Counters;
using NBench.Sdk;

namespace NBench.Metrics.Counters
{
    public class CounterMeasurementConfigurator : MeasurementConfigurator<CounterMeasurementAttribute>
    {
        public static readonly CounterSelector Selector = new CounterSelector();

        public override MetricName GetName(CounterMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            return MetricNames.CustomCounter;
        }

        public override MetricsCollectorSelector GetMetricsProvider(CounterMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            return Selector;
        }

        public override IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(CounterMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            var throughputAssertion = instance as CounterThroughputAssertionAttribute;
            var totalAssertion = instance as CounterTotalAssertionAttribute;
            if (throughputAssertion != null)
            {
                return new[]
                {
                    new CounterBenchmarkSetting(instance.CounterName, AssertionType.Throughput,
                        new Assertion(throughputAssertion.Condition, throughputAssertion.AverageOperationsPerSecond,
                            throughputAssertion.MaxAverageOperationsPerSecond))
                };
            }
            if (totalAssertion != null)
            {
                return new[]
                {
                    new CounterBenchmarkSetting(instance.CounterName, AssertionType.Total,
                        new Assertion(totalAssertion.Condition, totalAssertion.AverageOperationsTotal,
                            totalAssertion.MaxAverageOperationsTotal))
                };
            }

            return new[] {new CounterBenchmarkSetting(instance.CounterName, AssertionType.Total, Assertion.Empty)};
        }
    }
}

