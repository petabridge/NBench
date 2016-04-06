// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NBench.Sdk;

namespace NBench.Metrics.GarbageCollection
{
    /// <summary>
    /// Used inside a <see cref="BenchmarkSettings"/> class to indiciate that a specific <see cref="GcMetric"/>
    /// for a <see cref="GcGeneration"/> needs to be recorded and tested against <see cref="Assertion"/>.
    /// </summary>
    public sealed class GcBenchmarkSetting : IBenchmarkSetting
    {
        public GcBenchmarkSetting(GcMetric metric, GcGeneration generation, AssertionType assertionType, Assertion assertion)
        {
            AssertionType = assertionType;
            Assertion = assertion;
            GcMetricName = new GcMetricName(metric, generation);
        }

        public GcMetric Metric => GcMetricName.Metric;

        public GcGeneration Generation => GcMetricName.Generation;

        public GcMetricName GcMetricName { get; }

        public MetricName MetricName => GcMetricName;
        public AssertionType AssertionType { get; }

        public Assertion Assertion { get; }

        private bool Equals(GcBenchmarkSetting other)
        {
            return Metric == other.Metric 
                   && Generation == other.Generation 
                   && AssertionType == other.AssertionType 
                   && Assertion.Equals(other.Assertion);
        }

        public bool Equals(IBenchmarkSetting other)
        {
            return other is GcBenchmarkSetting &&
                   Equals((GcBenchmarkSetting) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GcBenchmarkSetting && Equals((GcBenchmarkSetting) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Metric;
                hashCode = (hashCode*397) ^ (int) Generation;
                hashCode = (hashCode*397) ^ (int) AssertionType;
                hashCode = (hashCode*397) ^ Assertion.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GcBenchmarkSetting left, GcBenchmarkSetting right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GcBenchmarkSetting left, GcBenchmarkSetting right)
        {
            return !Equals(left, right);
        }

        internal class GcBenchmarkDistinctComparer : IEqualityComparer<GcBenchmarkSetting>
        {
            private GcBenchmarkDistinctComparer() { }

            public static readonly GcBenchmarkDistinctComparer Instance = new GcBenchmarkDistinctComparer();

            public bool Equals(GcBenchmarkSetting x, GcBenchmarkSetting y)
            {
                return x.Metric.Equals(y.Metric) && x.Generation.Equals(y.Generation);
            }

            public int GetHashCode(GcBenchmarkSetting obj)
            {
                unchecked
                {
                    var hashCode = (int)obj.Metric;
                    hashCode = (hashCode * 397) ^ (int)obj.Generation;
                    return hashCode;
                }
            }
        }
    }
}

