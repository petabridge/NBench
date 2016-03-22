using System;
using System.Collections.Generic;
using System.Diagnostics;
using NBench.Metrics;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    /// Used to warm up and cache <see cref="PerformanceCounter"/> instances
    /// </summary>
    internal sealed class PerformanceCounterCache
    {
       private readonly IDictionary<MetricName, CachedPerformanceCounterProxy> _cachedCounters;

        public PerformanceCounterCache() : this(new Dictionary<MetricName, CachedPerformanceCounterProxy>()) { }

        public PerformanceCounterCache(IDictionary<MetricName, CachedPerformanceCounterProxy> cachedCounters)
        {
            _cachedCounters = cachedCounters;
        }

        public bool Exists(MetricName name)
        {
            // exists and is not disposed
            return _cachedCounters.ContainsKey(name) && !_cachedCounters[name].WasDisposed;
        }

        public bool Put(MetricName name, IPerformanceCounterProxy concreteCounter)
        {
            if (!Exists(name))
            {
                _cachedCounters[name] = new CachedPerformanceCounterProxy(() => concreteCounter);
                return true;
            }

            // already exists - not overriding
            return false;
        }

        public IPerformanceCounterProxy Get(MetricName name)
        {
            if (!Exists(name))
            {
                return EmptyPerformanceCounterProxy.Instance;
            }
            var counter = _cachedCounters[name];
            counter.Touch(); // increment reference count
            return counter;
        }
    }
}
