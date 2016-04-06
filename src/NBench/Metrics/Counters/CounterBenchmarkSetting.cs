// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NBench.Sdk;

namespace NBench.Metrics.Counters
{
    /// <summary>
    /// Used inside a <see cref="BenchmarkSettings"/> class to indiciate that a specific <see cref="Counter"/>
    /// needs to be recorded and tested against <see cref="Assertion"/>.
    /// </summary>
    public sealed class CounterBenchmarkSetting : IBenchmarkSetting
    {
        public CounterBenchmarkSetting(string counterName, AssertionType assertionType, Assertion assertion)
        {
            Contract.Requires(!string.IsNullOrEmpty(counterName));
            CounterName = new CounterMetricName(counterName);
            AssertionType = assertionType;
            Assertion = assertion;
        }

        public CounterMetricName CounterName { get; }

        public MetricName MetricName => CounterName;

        public AssertionType AssertionType { get; }

        public Assertion Assertion { get; }

        private bool Equals(CounterBenchmarkSetting other)
        {
            return string.Equals(CounterName, other.CounterName) 
                   && AssertionType == other.AssertionType 
                   && Assertion.Equals(other.Assertion);
        }

        public bool Equals(IBenchmarkSetting other)
        {
            return other is CounterBenchmarkSetting &&
                   Equals((CounterBenchmarkSetting) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CounterBenchmarkSetting && Equals((CounterBenchmarkSetting) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CounterName.GetHashCode();
                hashCode = (hashCode*397) ^ (int) AssertionType;
                hashCode = (hashCode*397) ^ Assertion.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(CounterBenchmarkSetting left, CounterBenchmarkSetting right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CounterBenchmarkSetting left, CounterBenchmarkSetting right)
        {
            return !Equals(left, right);
        }

        internal class CounterBenchmarkDistinctComparer : IEqualityComparer<CounterBenchmarkSetting>
        {
            private CounterBenchmarkDistinctComparer() { }

            public static readonly CounterBenchmarkDistinctComparer Instance = new CounterBenchmarkDistinctComparer();

            public bool Equals(CounterBenchmarkSetting x, CounterBenchmarkSetting y)
            {
                return x.CounterName.Equals(y.CounterName);
            }

            public int GetHashCode(CounterBenchmarkSetting obj)
            {
                return obj.CounterName.GetHashCode();
            }
        }
    }
}

