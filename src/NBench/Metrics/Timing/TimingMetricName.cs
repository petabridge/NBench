// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;

namespace NBench.Metrics.Timing
{
    /// <summary>
    /// <see cref="MetricName"/> used for <see cref="ElapsedTimeAssertionAttribute"/> and <see cref="TimingMeasurementAttribute"/>.
    /// </summary>
    public sealed class TimingMetricName : MetricName
    {
        private readonly string _name;

        public const string DefaultName = "Elapsed Time";

        public static readonly TimingMetricName Default = new TimingMetricName();

        public TimingMetricName() : this(DefaultName) { }

        public TimingMetricName(string name)
        {
            Contract.Requires(name != null);
            _name = name;
        }

        private bool Equals(TimingMetricName other)
        {
            return string.Equals(_name, other._name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is TimingMetricName && Equals((TimingMetricName) obj);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public static bool operator ==(TimingMetricName left, TimingMetricName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TimingMetricName left, TimingMetricName right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(MetricName other)
        {
            return (other is TimingMetricName) && Equals((TimingMetricName) other);
        }

        public override string ToHumanFriendlyString()
        {
            return _name;
        }

        public override string ToString()
        {
            return ToHumanFriendlyString();
        }
    }
}

