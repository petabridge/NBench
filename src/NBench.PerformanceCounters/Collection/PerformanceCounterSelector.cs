using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using NBench.Collection;
using NBench.Metrics;
using NBench.PerformanceCounters.Metrics;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    /// Factory that actually creates <see cref="PerformanceCounter"/> instances internally
    /// to correspond with each <see cref="PerformanceCounterBenchmarkSetting"/>
    /// </summary>
    public class PerformanceCounterSelector : MetricsCollectorSelector
    {
        /// <summary>
        /// Maximum number of times we can recreate a faulted performance counter
        /// </summary>
        public const int MaximumCounterRestarts = 10;

        public PerformanceCounterSelector() : this(PerformanceCounterMetricName.DefaultName) { }

        public PerformanceCounterSelector(MetricName name) : base(name)
        {
        }

        public PerformanceCounterSelector(MetricName name, SysInfo systemInfo) : base(name, systemInfo)
        {
        }

        public override MetricCollector Create(RunMode runMode, WarmupData warmup, IBenchmarkSetting setting)
        {
            Contract.Assert(setting != null);
            Contract.Assert(setting is PerformanceCounterBenchmarkSetting);
            var counterBenchmarkSetting = setting as PerformanceCounterBenchmarkSetting;
            var name = counterBenchmarkSetting.PerformanceCounterMetric;

            /*
             * TODO
             * We need to collect the correct metric from the PerformanceCounter, and that depends on the
             * PerformanceCounterType enumeration provided back from the PerformanceCounter object we instantiate.
             *
             * So we'll switch the implementation out based on the type of counter, with the default value being a raw value collector.
             */
            var proxy = new PerformanceCounterProxy(MaximumCounterRestarts, () => new PerformanceCounter(name.CategoryName, name.CounterName,
                name.InstanceName ?? string.Empty, true));
            Thread.SpinWait(10); // wait a bit before we attempt to use the counter right away
            if (!proxy.CanWarmup)
            {
                throw new NBenchException($"Failed to create PerformanceCounterMeasurement {name.ToHumanFriendlyString()} - internal error while creating performance counter.");
            }
            return new PerformanceCounterRawValueCollector(name, name.UnitName ?? MetricNames.DefaultUnitName, proxy, true);

        }
    }
}
