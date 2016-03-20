// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Metrics.Memory
{
    /// <summary>
    /// Name for memory metrics
    /// </summary>
    public class MemoryMetricName : MetricName
    {
        public MemoryMetricName(MemoryMetric metric)
        {
            Metric = metric;
        }

        public MemoryMetric Metric { get; }


        protected bool Equals(MemoryMetricName other)
        {
            return Metric == other.Metric;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MemoryMetricName) obj);
        }

        public override int GetHashCode()
        {
            return (int) Metric;
        }

        public override bool Equals(MetricName other)
        {
            return other is MemoryMetricName && Equals((MemoryMetricName)other);
        }

        public override string ToHumanFriendlyString()
        {
            return ToString();
        }

        public static bool operator ==(MemoryMetricName left, MemoryMetricName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MemoryMetricName left, MemoryMetricName right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Metric.ToString();
        }
    }
}

