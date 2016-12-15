// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace NBench.Sdk
{
    /// <summary>
    /// Used when estimating the size of the next run.
    /// 
    /// Designed to help buffer NBench from influencing any benchmarks itself.
    /// </summary>
    public struct WarmupData
    {
        /// <summary>
        /// The default sample size we use during JIT warmups
        /// </summary>
        public const int PreWarmupSampleSize = 1;

        public const int WarmupSampleSize = 1 << 10;

        public WarmupData(long ticks, long actualRunsMeasured)
        {
            Contract.Requires(ticks > 0);
            Contract.Requires(actualRunsMeasured > 0);
            ElapsedTicks = ticks;
            ElapsedNanos = ElapsedTicks / (double)Stopwatch.Frequency* 1000000000;
            ElapsedSeconds = ElapsedNanos / 1000000000;
            ActualRunsMeasured = actualRunsMeasured;
            NanosPerRun = ElapsedNanos/actualRunsMeasured;
            EstimatedRunsPerSecond = (long)Math.Ceiling(1000000000/NanosPerRun);
        }

        public double NanosPerRun { get; }

        public long ElapsedTicks { get; }

        public double ElapsedNanos { get; }

        public double ElapsedSeconds { get; }

        public long EstimatedRunsPerSecond { get; }

        public long ActualRunsMeasured { get; }

        public static readonly WarmupData PreWarmup = new WarmupData(TimeSpan.FromSeconds(1).Ticks, PreWarmupSampleSize);

        public static readonly WarmupData DefaultWarmup = new WarmupData(TimeSpan.FromSeconds(1).Ticks, PreWarmupSampleSize);

        public bool Equals(WarmupData other)
        {
            return ElapsedTicks == other.ElapsedTicks && ActualRunsMeasured == other.ActualRunsMeasured;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is WarmupData && Equals((WarmupData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ElapsedTicks.GetHashCode()*397) ^ ActualRunsMeasured.GetHashCode();
            }
        }

        public static bool operator ==(WarmupData left, WarmupData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WarmupData left, WarmupData right)
        {
            return !left.Equals(right);
        }
    }
}

