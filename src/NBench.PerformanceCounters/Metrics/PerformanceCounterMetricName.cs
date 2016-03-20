using System;
using System.Diagnostics.Contracts;
using NBench.Metrics;

namespace NBench.PerformanceCounters.Metrics
{
    /// <summary>
    /// Uniquely represents a performance counter measurement name inside our collections engine
    /// </summary>
    public sealed class PerformanceCounterMetricName : MetricName, IEquatable<PerformanceCounterMetricName>
    {
        public static readonly PerformanceCounterMetricName DefaultName = new PerformanceCounterMetricName("Category", "Instance", "Instance", "Units");

        public PerformanceCounterMetricName(string categoryName, string counterName, string instanceName, string unitName)
        {
            Contract.Requires(categoryName != null);
            Contract.Requires(counterName != null);
            CategoryName = categoryName;
            CounterName = counterName;
            InstanceName = instanceName;
            UnitName = unitName;
        }

        /// <summary>
        /// The name of the performance counter category with which this performance counter is associated.
        /// </summary>
        public string CategoryName { get; }

        /// <summary>
        /// The name of the performance counter.
        /// </summary>
        public string CounterName { get; }

        /// <summary>
        /// The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.
        /// </summary>
        public string InstanceName { get; }

        /// <summary>
        /// Human-readable name of the measurement units associated with this performance counter.
        /// 
        /// Used solely for reporting purposes - designed to make it easier to understand what the metric reads.
        /// For instance, if this performance counter was measuring disk writes the unit name would be "bytes"
        /// Or if this performance counter were measuring page faults, the unit name would be "page faults"
        /// </summary>
        public string UnitName { get; }

        public override bool Equals(MetricName other)
        {
            return other is PerformanceCounterMetricName 
                && Equals((PerformanceCounterMetricName) other);
        }

        public bool Equals(PerformanceCounterMetricName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(CategoryName, other.CategoryName) 
                && string.Equals(CounterName, other.CounterName) 
                && string.Equals(InstanceName, other.InstanceName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PerformanceCounterMetricName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CategoryName?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (CounterName?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (InstanceName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public static bool operator ==(PerformanceCounterMetricName left, PerformanceCounterMetricName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PerformanceCounterMetricName left, PerformanceCounterMetricName right)
        {
            return !Equals(left, right);
        }

        public override string ToHumanFriendlyString()
        {
            if(!string.IsNullOrEmpty(InstanceName))
                return $"[PerformanceCounter] {CategoryName}:{CounterName}:{InstanceName}";
            return $"[PerformanceCounter] {CategoryName}:{CounterName}";
        }
    }
}
