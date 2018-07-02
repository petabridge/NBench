// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Util;

namespace NBench
{
    /// <summary>
    /// A high-performance, thread-safe counter class used to measure throughput on user-defined metrics
    /// </summary>
    public sealed class Counter
    {
        private readonly AtomicCounter _internalCounter;
        public CounterMetricName Name { get; }

        internal Counter(AtomicCounter internalCounter, CounterMetricName name)
        {
            _internalCounter = internalCounter;
            Name = name;
        }

        /// <summary>
        /// Increment the value of the counter by 1
        /// </summary>
        public void Increment()
        {
            _internalCounter.Increment();
        }

        /// <summary>
        /// Increment the counter by a user-defined amount.
        /// </summary>
        /// <param name="v">The counter increment value.</param>
        public void Increment(long v)
        {
            _internalCounter.Increment(v);
        }

        /// <summary>
        /// Decrement the value of the counter by 1
        /// </summary>
        public void Decrement()
        {
            _internalCounter.Decrement();
        }

        /// <summary>
        /// Decrement the counter by a user-defined amount.
        /// </summary>
        /// <param name="v">The counter decrement value.</param>
        public void Decrement(long v)
        {
            _internalCounter.Decrement(v);
        }

        /// <summary>
        /// Current value of the counter
        /// </summary>
        public long Current => _internalCounter.Current;
    }
}

