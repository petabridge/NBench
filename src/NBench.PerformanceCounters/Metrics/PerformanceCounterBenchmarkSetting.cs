// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using NBench.Metrics;
using NBench.Sdk;

namespace NBench.PerformanceCounters.Metrics
{
    /// <summary>
    /// <see cref="IBenchmarkSetting"/> used to instrument performance counters inside a <see cref="Benchmark"/>
    /// </summary>
    public sealed class PerformanceCounterBenchmarkSetting : MarshalByRefObject, IBenchmarkSetting, IEquatable<PerformanceCounterBenchmarkSetting>
    {
        public PerformanceCounterBenchmarkSetting(PerformanceCounterMetricName performanceCounterMetric, AssertionType assertionType, Assertion assertion)
        {
            PerformanceCounterMetric = performanceCounterMetric;
            AssertionType = assertionType;
            Assertion = assertion;
        }

        public PerformanceCounterMetricName PerformanceCounterMetric { get; }

        public MetricName MetricName => PerformanceCounterMetric;
        public AssertionType AssertionType { get; }
        public Assertion Assertion { get; }

        public bool Equals(IBenchmarkSetting other)
        {
            return (other is PerformanceCounterBenchmarkSetting) 
                && Equals((PerformanceCounterBenchmarkSetting) other);
        }
        

        public bool Equals(PerformanceCounterBenchmarkSetting other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PerformanceCounterMetric.Equals(other.PerformanceCounterMetric) 
                && AssertionType == other.AssertionType 
                && Assertion.Equals(other.Assertion);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PerformanceCounterBenchmarkSetting) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PerformanceCounterMetric.GetHashCode();
                hashCode = (hashCode*397) ^ (int) AssertionType;
                hashCode = (hashCode*397) ^ Assertion.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PerformanceCounterBenchmarkSetting left, PerformanceCounterBenchmarkSetting right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PerformanceCounterBenchmarkSetting left, PerformanceCounterBenchmarkSetting right)
        {
            return !Equals(left, right);
        }
    }
}

