using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
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
        private readonly PerformanceCounterCache _cache = new PerformanceCounterCache();

        public PerformanceCounterSelector() : this(PerformanceCounterMetricName.DefaultName) { }

        public PerformanceCounterSelector(MetricName name) : base(name)
        {
        }

        public PerformanceCounterSelector(MetricName name, SysInfo systemInfo) : base(name, systemInfo)
        {
        }

        public static bool CanFindPerformanceCounter(PerformanceCounterMetricName name)
        {
            var hasInstance = !string.IsNullOrEmpty(name.InstanceName);
            return hasInstance
                ? PerformanceCounterCategory.InstanceExists(name.InstanceName, name.CategoryName)
                : PerformanceCounterCategory.CounterExists(name.CounterName, name.CategoryName);
        }

        public override MetricCollector Create(RunMode runMode, WarmupData warmup, IBenchmarkSetting setting)
        {
            Contract.Assert(setting != null);
            Contract.Assert(setting is PerformanceCounterBenchmarkSetting);
            var counterBenchmarkSetting = setting as PerformanceCounterBenchmarkSetting;
            var name = counterBenchmarkSetting.PerformanceCounterMetric;

            // re-use the PerformanceCounter objects in our pool if possible
            if (_cache.Exists(name))
                return new PerformanceCounterValueCollector(name, name.UnitName ?? MetricNames.DefaultUnitName, _cache.Get(name), true);

            // otherwise, warm up new ones
            var maxRetries = 3;
            var currentRetries = 0;

            if (!PerformanceCounterCategory.CounterExists(name.CounterName, name.CategoryName))
                throw new NBenchException($"Performance counter {name.ToHumanFriendlyString()} is not registered on this machine. Please create it first.");

            // check to see that the instance we're interested in is registered
            if (!string.IsNullOrEmpty(name.InstanceName))
            {
                var categories = PerformanceCounterCategory.GetCategories().Where(x => x.CategoryType == PerformanceCounterCategoryType.MultiInstance).ToList();
#if DEBUG
                Console.WriteLine("---- DEBUG -----");
                Console.WriteLine("{0} multi-instance categories detected", categories.Count);
#endif
                var category = categories.Single(x => x.CategoryName == name.CategoryName);
                var instances = category.GetInstanceNames();

                if (!instances.Contains(name.InstanceName))
                {
#if DEBUG
                    Console.WriteLine("---- DEBUG -----");
                    Console.WriteLine("Multi-instance? {0}", category.CategoryType);
                    foreach (var instance in instances)
                        Console.WriteLine(instance);
#endif
                    throw new NBenchException($"Performance counter {name.CategoryName}:{name.CounterName} exists, but we could not find an instance {name.InstanceName}.");
                }

            }

            var proxy = new PerformanceCounterProxy(() =>
            {
                var counter = new PerformanceCounter(name.CategoryName, name.CounterName,
                    name.InstanceName ?? string.Empty, true);

                return counter;
            });
            while (!CanFindPerformanceCounter(name) && currentRetries <= maxRetries)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(1000 + 100 * (currentRetries ^ 2))); // little bit of exponential backoff
                if (proxy.CanWarmup)
                    break;
                currentRetries++;
            }

            if (!proxy.CanWarmup)
                throw new NBenchException($"Performance counter {name.ToHumanFriendlyString()} is not registered on this machine. Please create it first.");

            // cache this performance counter and pool it for re-use
            _cache.Put(name, proxy);
            return new PerformanceCounterValueCollector(name, name.UnitName ?? MetricNames.DefaultUnitName, _cache.Get(name), true);
        }
    }
}
