using System;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    /// Special case pattern - used when we can't find a <see cref="IPerformanceCounterProxy"/>
    /// inside the <see cref="PerformanceCounterCache"/>.
    /// </summary>
    public class EmptyPerformanceCounterProxy : IPerformanceCounterProxy
    {
        private EmptyPerformanceCounterProxy() { }

        public static readonly EmptyPerformanceCounterProxy Instance = new EmptyPerformanceCounterProxy();

        public void Dispose()
        {
            
        }

        public bool WasDisposed => true;
        public bool CanWarmup => false;
        public double Collect()
        {
            throw new NotImplementedException("Should not have had an EmptyPerformanceCounterProxy make it into benchmark");
        }
    }
}