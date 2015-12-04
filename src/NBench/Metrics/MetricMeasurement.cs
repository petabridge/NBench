// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench.Metrics
{
    /// <summary>
    /// A single recorded value inside a <see cref="MeasureBucket"/>
    /// </summary>
    public struct MetricMeasurement
    {
        public MetricMeasurement(long elapsedTicks, long metricValue)
        {
            ElapsedTicks = elapsedTicks;
            MetricValue = metricValue;
        }

        public long ElapsedTicks { get; }

        public long MetricValue { get; }

        public bool Equals(MetricMeasurement other)
        {
            return ElapsedTicks == other.ElapsedTicks && MetricValue == other.MetricValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MetricMeasurement && Equals((MetricMeasurement) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ElapsedTicks.GetHashCode()*397) ^ MetricValue.GetHashCode();
            }
        }

        public static bool operator ==(MetricMeasurement left, MetricMeasurement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MetricMeasurement left, MetricMeasurement right)
        {
            return !left.Equals(right);
        }
    }
}

