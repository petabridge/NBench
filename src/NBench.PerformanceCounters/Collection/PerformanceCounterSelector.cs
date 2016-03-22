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

        private readonly PerformanceCounterCache _cache = new PerformanceCounterCache();

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

            var counterExists = PerformanceCounterCategory.CounterExists(name.CounterName, name.CategoryName);

            // re-use the PerformanceCounter objects in our pool if possible
            if(_cache.Exists(name))
                return new PerformanceCounterRawValueCollector(name, name.UnitName ?? MetricNames.DefaultUnitName, _cache.Get(name), true);

            // otherwise, warm up new ones
            var retries = 5;
            var proxy = new PerformanceCounterProxy(MaximumCounterRestarts, () => new PerformanceCounter(name.CategoryName, name.CounterName,
                name.InstanceName ?? string.Empty, true));
            while (
                ((!string.IsNullOrEmpty(name.InstanceName) 
                && !PerformanceCounterCategory.InstanceExists(name.InstanceName, name.CategoryName)) || PerformanceCounterCategory.CounterExists(name.CounterName, name.CategoryName))
                && --retries > 0)
            {
                Thread.Sleep(1000);
                if (proxy.CanWarmup)
                    break;
            }

            if(!proxy.CanWarmup)
                throw new NBenchException($"Performance counter {name.ToHumanFriendlyString()} is not registered on this machine. Please create it first.");

            // cache this performance counter and pool it for re-use
            _cache.Put(name, proxy);
            return new PerformanceCounterRawValueCollector(name, name.UnitName ?? MetricNames.DefaultUnitName, _cache.Get(name), true);
        }
    }
}
