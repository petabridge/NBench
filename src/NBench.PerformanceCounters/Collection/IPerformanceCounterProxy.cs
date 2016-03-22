using System;
using System.Diagnostics;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    /// Proxy wrapper for <see cref="PerformanceCounter"/>s - designed to help mitigate
    /// against "slow start" issues for counters.
    /// </summary>
    public interface IPerformanceCounterProxy : IDisposable
    {
        /// <summary>
        /// Have we disposed this counter already?
        /// </summary>
        bool WasDisposed { get; }

        /// <summary>
        /// A flag that indicates of the underlying counter was able to start successfully
        /// </summary>
        bool CanWarmup { get; }

        /// <summary>
        /// Gets the raw value from the underlying <see cref="PerformanceCounter"/>
        /// </summary>
        /// <returns>A <see cref="long"/> representing the counter's value</returns>
        double Collect();
    }
}