// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

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
        /// The default sample size we use during warmups
        /// </summary>
        public const int WarmupSampleSize = 1 << 10;

        public WarmupData(TimeSpan elapsedTime, int numberOfObservedSamples)
        {
            ElapsedTime = elapsedTime;
            NumberOfObservedSamples = numberOfObservedSamples;
        }

        public TimeSpan ElapsedTime { get; }

        public int NumberOfObservedSamples { get; }

        public static readonly WarmupData Empty = new WarmupData(TimeSpan.Zero, WarmupSampleSize);

        public bool Equals(WarmupData other)
        {
            return ElapsedTime.Equals(other.ElapsedTime) && NumberOfObservedSamples == other.NumberOfObservedSamples;
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
                return (ElapsedTime.GetHashCode()*397) ^ NumberOfObservedSamples.GetHashCode();
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

