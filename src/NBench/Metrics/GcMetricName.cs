// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Metrics
{
    public class GcMetricName : MetricName
    {
        public GcMetricName(GcMetric metric, GcGeneration generation)
        {
            Metric = metric;
            Generation = generation;
        }

        public GcMetric Metric { get; }

        public GcGeneration Generation { get; }

        public override string ToString()
        {
            return $"{Metric} [{Generation}]";
        }

        protected bool Equals(GcMetricName other)
        {
            return Metric == other.Metric && Generation == other.Generation;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GcMetricName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Metric*397) ^ (int) Generation;
            }
        }

        public static bool operator ==(GcMetricName left, GcMetricName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GcMetricName left, GcMetricName right)
        {
            return !Equals(left, right);
        }
    }
}

