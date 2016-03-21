using System;
using System.Threading;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    /// Cached, reference counted implementation of <see cref="IPerformanceCounterProxy"/>
    /// </summary>
    public class CachedPerformanceCounterProxy : IPerformanceCounterProxy
    {

        private readonly Func<IPerformanceCounterProxy> _getUnderlying;
        private int _referenceCount;
        public int CurrentReferenceCount => _referenceCount;

        public CachedPerformanceCounterProxy(Func<IPerformanceCounterProxy> getUnderlying)
        {
            _getUnderlying = getUnderlying;
        }

        public void Dispose()
        {
            if (CurrentReferenceCount <= 0)
            {
                _getUnderlying()?.Dispose();
            }
            else
            {
                Release();
            }
        }

        public bool WasDisposed => _getUnderlying().WasDisposed;
        public bool CanWarmup => _getUnderlying().CanWarmup;

        public long Collect()
        {
            return _getUnderlying().Collect();
        }

        public void Touch()
        {
            Interlocked.Increment(ref _referenceCount);
        }

        public void Release()
        {
            Interlocked.Decrement(ref _referenceCount);
        }
    }
}