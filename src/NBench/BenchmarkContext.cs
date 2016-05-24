// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Tracing;

namespace NBench
{
    /// <summary>
    /// Contains the runtime context for a given benchmark run.
    /// 
    /// Makes it possible for developers to access built-in <see cref="Counter"/>s declared
    /// via the <see cref="CounterMeasurementAttribute"/>, <see cref="CounterThroughputAssertionAttribute"/>, 
    /// and <see cref="CounterTotalAssertionAttribute"/> classes.
    /// </summary>
    public sealed class BenchmarkContext
    {
        private readonly IReadOnlyDictionary<string, Counter> _counters;

        /// <summary>
        /// Empty context - used for unit testing.
        /// </summary>
        internal static readonly BenchmarkContext Empty = new BenchmarkContext(new Dictionary<CounterMetricName, Counter>(), NoOpBenchmarkTrace.Instance);

        public BenchmarkContext(IReadOnlyDictionary<CounterMetricName, Counter> counters, IBenchmarkTrace trace)
        {
            Trace = trace;
            _counters = counters.ToDictionary(k => k.Key.CounterName, v => v.Value);
        }

        /// <summary>
        /// Retrieves a named <see cref="Counter"/> instance that has already been registered via a <see cref="CounterMeasurementAttribute"/>, 
        /// <see cref="CounterThroughputAssertionAttribute"/>, or <see cref="CounterTotalAssertionAttribute"/> classes.
        /// </summary>
        /// <param name="name">The name of the counter.</param>
        /// <returns>The corresponding <see cref="Counter"/> instance.</returns>
        public Counter GetCounter(string name)
        {
            try
            {
                return _counters[name];
            }
            catch (Exception ex)
            {
                throw new NBenchException("error while retrieving counter", ex);
            }
        }

        /// <summary>
        /// Determines if a counter with a particular name has been registered or not.
        /// </summary>
        /// <param name="name">The name of the counter.</param>
        /// <returns><c>true</c> if it's been registered, <c>false</c> otherwise.</returns>
        public bool CounterExists(string name)
        {
            return _counters.ContainsKey(name);
        }

        /// <summary>
        /// The names of all available counters
        /// </summary>
        public IEnumerable<string> CounterNames => _counters.Keys;
        
        /// <summary>
        /// All available counters
        /// </summary>
        public IEnumerable<Counter> Counters => _counters.Values;

        /// <summary>
        /// Allows NBench users to write custom messages directly into the NBench output.
        /// </summary>
        public IBenchmarkTrace Trace { get; }
    }
}

