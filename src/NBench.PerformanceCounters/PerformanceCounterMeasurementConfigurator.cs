using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using NBench.Collection;
using NBench.Metrics;
using NBench.PerformanceCounters.Collection;
using NBench.PerformanceCounters.Metrics;
using NBench.Sdk;

namespace NBench.PerformanceCounters
{
    /// <summary>
    /// Used to configure and instrument <see cref="PerformanceCounterMeasurementAttribute"/> instances inside a <see cref="Benchmark"/>
    /// </summary>
    public class PerformanceCounterMeasurementConfigurator : MeasurementConfigurator<PerformanceCounterMeasurementAttribute>
    {
        public static readonly PerformanceCounterSelector Selector = new PerformanceCounterSelector();

        public override MetricName GetName(PerformanceCounterMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);

            // Grab a dynamic value for the instance name of this performance counter
            var instanceName = string.Equals(instance.InstanceName, NBenchPerformanceCounterConstants.CurrentProcessName)
                ? Process.GetCurrentProcess().ProcessName
                : instance.InstanceName;

            return new PerformanceCounterMetricName(instance.CategoryName, instance.CounterName, instanceName, instance.UnitName);
        }

        public override MetricsCollectorSelector GetMetricsProvider(PerformanceCounterMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            return Selector;
        }

        public override IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(PerformanceCounterMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            var throughputAssertion = instance as PerformanceCounterThroughputAssertionAttribute;
            var totalAssertion = instance as PerformanceCounterTotalAssertionAttribute;

            var name = GetName(instance) as PerformanceCounterMetricName;
            Contract.Assert(name != null);

            if (throughputAssertion != null)
            {
                return new[]
                {
                    new PerformanceCounterBenchmarkSetting(name, AssertionType.Throughput,
                        new Assertion(throughputAssertion.Condition, throughputAssertion.AverageValuePerSecond,
                            throughputAssertion.MaxAverageValuePerSecond))
                };
            }
            if (totalAssertion != null)
            {
                return new[]
                {
                     new PerformanceCounterBenchmarkSetting(name, AssertionType.Total,
                        new Assertion(totalAssertion.Condition, totalAssertion.AverageValueTotal,
                           totalAssertion.MaxAverageValueTotal))
                };
            }

            return new[]
            {
                new PerformanceCounterBenchmarkSetting(name, AssertionType.Throughput, Assertion.Empty)
            };
        }
    }
}
