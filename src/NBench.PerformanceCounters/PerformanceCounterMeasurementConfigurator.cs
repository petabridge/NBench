using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
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

        public static readonly Lazy<string> ProcessId = new Lazy<string>(GetProcessId, true);

        /// <summary>
        /// Due to a bunch of fun hijinks around multiple processes all having the same name, we
        /// have to do some extra legwork to find the correct performance counter instance name that corresponds
        /// to this specific instance of NBench.
        /// </summary>
        public static string GetProcessId()
        {
            var baseName = Process.GetCurrentProcess().ProcessName;
            var pid = Process.GetCurrentProcess().Id;
            var processIndex = 0; //the very first process doesn't have a "#N" appended to the back of it
            var expectedCounterName = processIndex == 0 ? baseName : baseName + "#" + processIndex;
            while (true)
            {
                try
                {
                    var pc = new PerformanceCounter("Process", "ID Process", expectedCounterName, true);
                    if (pid == (int) pc.NextValue())
                    {
                        break;
                    }

                    processIndex++;
                    expectedCounterName = processIndex == 0 ? baseName : baseName + "#" + processIndex;
                    try { pc.Dispose(); }
                    catch { } //supress exceptions
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains($"{expectedCounterName} does not exist in the specified Category"))
                    {
                        break;
                    }
                    throw;
                }
            }

            return expectedCounterName;
        }

        public override MetricName GetName(PerformanceCounterMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);

            // Grab a dynamic value for the instance name of this performance counter
            var instanceName = string.Equals(instance.InstanceName, NBenchPerformanceCounterConstants.CurrentProcessName)
                ? ProcessId.Value
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
