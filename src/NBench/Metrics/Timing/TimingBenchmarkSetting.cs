// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Sdk;

namespace NBench.Metrics.Timing
{
    /// <summary>
    /// <see cref="IBenchmarkSetting"/> implementation for <see cref="TimingMeasurementAttribute"/>
    /// and other derived versions.
    /// </summary>
    public sealed class TimingBenchmarkSetting : IBenchmarkSetting
    {
        public TimingBenchmarkSetting(TimingMetricName metricName, Assertion assertion)
        {
            TimingMetricName = metricName;
            Assertion = assertion;
            AssertionType = AssertionType.Total; // timing specs always care about totals only
        }

        public bool Equals(IBenchmarkSetting other)
        {
            return (other is TimingBenchmarkSetting) 
                && Equals((TimingBenchmarkSetting) other);
        }

        public TimingMetricName TimingMetricName { get; }
        public MetricName MetricName => TimingMetricName;
        public AssertionType AssertionType { get; }
        public Assertion Assertion { get; }

        private bool Equals(TimingBenchmarkSetting other)
        {
            return MetricName.Equals(other.MetricName) && AssertionType == other.AssertionType && Assertion.Equals(other.Assertion);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is TimingBenchmarkSetting && Equals((TimingBenchmarkSetting) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MetricName.GetHashCode();
                hashCode = (hashCode*397) ^ (int) AssertionType;
                hashCode = (hashCode*397) ^ Assertion.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(TimingBenchmarkSetting left, TimingBenchmarkSetting right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TimingBenchmarkSetting left, TimingBenchmarkSetting right)
        {
            return !Equals(left, right);
        }
    }
}

