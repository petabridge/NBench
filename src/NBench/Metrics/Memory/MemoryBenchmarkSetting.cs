// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NBench.Sdk;

namespace NBench.Metrics.Memory
{
    /// <summary>
    /// Used inside a <see cref="BenchmarkSettings"/> class to indiciate that a specific <see cref="MemoryMetric"/>
    /// needs to be recorded and tested against <see cref="Assertion"/>.
    /// </summary>
    public sealed class MemoryBenchmarkSetting : IBenchmarkSetting
    {
        public MemoryBenchmarkSetting(MemoryMetric metric, Assertion assertion)
        {
            Metric = metric;
            Assertion = assertion;
            AssertionType = AssertionType.Total;
            MetricName = new MemoryMetricName(metric);
        }

        public MemoryMetric Metric { get; }

        public MetricName MetricName { get; }
        public AssertionType AssertionType { get; }
        public Assertion Assertion { get; }

        private bool Equals(MemoryBenchmarkSetting other)
        {
            return Metric == other.Metric && Assertion.Equals(other.Assertion);
        }

        public bool Equals(IBenchmarkSetting other)
        {
            return other is MemoryBenchmarkSetting &&
                   Equals((MemoryBenchmarkSetting) other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is MemoryBenchmarkSetting && Equals((MemoryBenchmarkSetting)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Metric * 397) ^ Assertion.GetHashCode();
            }
        }

        public static bool operator ==(MemoryBenchmarkSetting left, MemoryBenchmarkSetting right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MemoryBenchmarkSetting left, MemoryBenchmarkSetting right)
        {
            return !Equals(left, right);
        }

        internal class MemoryBenchmarkDistinctComparer : IEqualityComparer<MemoryBenchmarkSetting>
        {
            private MemoryBenchmarkDistinctComparer() { }

            public static readonly MemoryBenchmarkDistinctComparer Instance = new MemoryBenchmarkDistinctComparer();

            public bool Equals(MemoryBenchmarkSetting x, MemoryBenchmarkSetting y)
            {
                return x.Metric.Equals(y.Metric);
            }

            public int GetHashCode(MemoryBenchmarkSetting obj)
            {
                return obj.Metric.GetHashCode();
            }
        }
    }
}

