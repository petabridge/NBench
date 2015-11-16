// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NBench.Metrics;

namespace NBench.Util
{
    /// <summary>
    /// Extension methods for combining numeric dictionaries together
    /// in order to compute important statistics
    /// </summary>
    public static class DeltaCalculator
    {
        /// <summary>
        /// For each item in final, subtract its value by initial to get the final delta.
        /// </summary>
        /// <param name="initial"></param>
        /// <param name="final"></param>
        /// <returns></returns>
        public static IDictionary<T, long> DistanceFromStart<T>(this IDictionary<T, long> initial,
            IDictionary<T, long> final)
        {
            Contract.Requires(initial != null);
            Contract.Requires(final != null);
            Contract.Requires(initial.Keys.SequenceEqual(final.Keys), "both dictionaries must contain exactly the same keys");
            var deltas = new Dictionary<T, long>();
            var count = initial.Count;
            var iterator = initial.GetEnumerator();
            for (var i = 0; i < count; i++)
            {
                var key = iterator.Current.Key;
                var value = final[key] - initial[key];
                deltas.Add(key, value);
            }

            return deltas;
        }

        /// <summary>
        /// For each item in final, subtract its value by initial to get the final delta.
        /// </summary>
        /// <param name="initial"></param>
        /// <returns></returns>
        public static IDictionary<T, long> DistanceFromStart<T>(this IDictionary<T, long> initial)
        {
            Contract.Requires(initial != null);
            var deltas = new Dictionary<T, long>();
            var head = initial.First().Value;
            var count = initial.Count;
            foreach (var next in initial)
            {
                long delta = next.Value - head;
                deltas.Add(next.Key, delta);
            }

            return deltas;
        }

        public static IEnumerable<long> DistanceFromStart(this IEnumerable<long> initial)
        {
            Contract.Requires(initial != null);
            var list = initial.ToList();
            var deltas = new List<long>();
            var head = list.FirstOrDefault();
            var count = list.Count;
            foreach (var next in list)
            {
                long delta = next - head;
                deltas.Add(delta);
            }

            return deltas;
        }

        /// <summary>
        /// The algorithm here is as follows
        /// 
        /// Head of queue == metric taken at time 0
        /// Every other item in queue == metric taken after 0
        /// 
        /// We want to compute the value relative to time 0
        /// </summary>
        /// <param name="metrics">The raw data</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> where the key is the time of the sample and value is the delta</returns>
        public static IDictionary<TimeSpan, double> DistanceFromStart(this Queue<MetricMeasurement> metrics)
        {
            Contract.Requires(metrics != null);
            Contract.Requires(metrics.Count >= 2, "need at least 1 head value and 1 tail value");
            Dictionary<TimeSpan, double> returnValue = new Dictionary<TimeSpan, double>(metrics.Count);
            MetricMeasurement head = metrics.Peek();
            var remaining = metrics.Count;
            for (var i = 0; i < remaining; i++)
            {
                var next = metrics.Dequeue();
                double delta = next.MetricValue - head.MetricValue;
                returnValue.Add(next.Elapsed, delta);
            }
            return returnValue;
        }
    }
}

