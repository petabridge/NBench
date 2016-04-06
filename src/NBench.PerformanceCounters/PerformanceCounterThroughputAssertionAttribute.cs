// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace NBench.PerformanceCounters
{
    /// <summary>
    /// Performs an assertion against <see cref="PerformanceCounter"/> values collected over the course of a benchmark.
    /// 
    /// This asserts the PERFORMANCE COUNTER VALUES / SECOND averaged over all runs of a benchmark.
    /// </summary>
    public class PerformanceCounterThroughputAssertionAttribute : PerformanceCounterMeasurementAttribute
    {
        public PerformanceCounterThroughputAssertionAttribute(string categoryName, string counterName, MustBe condition, double averageValuePerSecond) : base(categoryName, counterName)
        {
            Condition = condition;
            AverageValuePerSecond = averageValuePerSecond;
        }

        /// <summary>
        /// The test we're going to perform against the collected value of <see cref="CounterMeasurementAttribute.CounterName"/>
        /// and <see cref="AverageValuePerSecond"/>.
        /// </summary>
        public MustBe Condition { get; }

        /// <summary>
        /// The value that will be compared against the collected metric for <see cref="CounterMeasurementAttribute.CounterName"/>.
        /// </summary>
        public double AverageValuePerSecond { get; }

        /// <summary>
        /// Used only on <see cref="MustBe.Between"/> comparisons. This is the upper bound of that comparison
        /// and <see cref="AverageValuePerSecond"/> is the lower bound.
        /// </summary>
        public long? MaxAverageValuePerSecond { get; set; }
    }
}

