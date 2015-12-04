// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;

namespace NBench.Metrics
{
    public class CounterMetricName : MetricName
    {
        public CounterMetricName(string counterName)
        {
            Contract.Requires(!string.IsNullOrEmpty(counterName));
            CounterName = counterName;
        }

        public string CounterName { get; }

        protected bool Equals(CounterMetricName other)
        {
            Contract.Requires(other != null);
            return string.Equals(CounterName, other.CounterName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CounterMetricName) obj);
        }

        public override int GetHashCode()
        {
            return CounterName.GetHashCode();
        }

        public static bool operator ==(CounterMetricName left, CounterMetricName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CounterMetricName left, CounterMetricName right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"[Counter] {CounterName}";
        }
    }
}

