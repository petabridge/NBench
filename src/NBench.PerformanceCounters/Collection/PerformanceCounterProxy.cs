using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace NBench.PerformanceCounters.Collection
{
    /// <summary>
    /// Concrete implementation of <see cref="IPerformanceCounterProxy"/> that recreates the underlying
    /// <see cref="PerformanceCounter"/> on fault up to a maxmium limit.
    /// 
    /// NOT THREAD SAFE.
    /// </summary>
    public class PerformanceCounterProxy : IPerformanceCounterProxy
    {
        /// <summary>
        /// Our instance of the concrete counter - will be recreated on fault
        /// </summary>
        private PerformanceCounter _counter;

        private readonly Func<PerformanceCounter> _counterFactory; // recreate the counter on error

        public int MaximumRestarts { get; }
        public int CurrentRestarts { get; private set; }

        public PerformanceCounterProxy(int maximumRestarts, Func<PerformanceCounter> counterFactory)
        {
            Contract.Requires(MaximumRestarts >= 1);
            Contract.Requires(counterFactory != null);
            MaximumRestarts = maximumRestarts;
            _counterFactory = counterFactory;
            CurrentRestarts = 0;
        }

        public bool WasDisposed { get; private set; }

        public bool CanWarmup
        {
            get
            {
                try
                {
                    // will invoke an exception if the counter was created too quickly
                    Collect();
                    return true;
                }
                catch (Exception ex)
                { 
                    return false;
                }
            }
        }

        /// <summary>
        /// Return the raw value of the counter
        /// </summary>
        /// <returns>A long integer representing <see cref="PerformanceCounter.RawValue"/></returns>
        public long Collect()
        {
            var counter = GetOrCreate();
            try
            {
                return counter.RawValue;
            }
            catch (Exception)
            {
                // recreate counter
                CurrentRestarts++;
                if (CurrentRestarts >= MaximumRestarts)
                {
                    throw;
                }
                DisposeCounter();
                return 0;
            }
        }

        private PerformanceCounter GetOrCreate()
        {
            if (_counter == null && CurrentRestarts < MaximumRestarts)
            {
                //create the counter
                _counter = _counterFactory();
            }

            return _counter;
        }

        public void Dispose(bool isDisposing)
        {
            if (isDisposing && !WasDisposed)
            {

                try
                {
                    DisposeCounter();
                }
                catch //supress errors - we do not care
                {
                }
                finally
                {
                    WasDisposed = true;
                }
            }
        }

        private void DisposeCounter()
        {
            _counter?.Close();
            _counter?.Dispose();
            _counter = null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}