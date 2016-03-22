// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench
{
    /// <summary>
    /// Creates a custom <see cref="Counter"/> to be used for tracking app-specific metrics
    /// in combination with a <see cref="PerfBenchmarkAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CounterMeasurementAttribute : MeasurementAttribute
    {
        public CounterMeasurementAttribute(string counterName)
        {
            CounterName = counterName;
        }

        /// <summary>
        /// The name of the <see cref="Counter"/> being measured
        /// </summary>
        public string CounterName { get; private set; }
    }

    /// <summary>
    /// Performs an assertion against counters collected over the course of a benchmark.
    /// 
    /// This asserts the NUMBER OF OPERATIONS / SECOND values averaged over all runs of a benchmark.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CounterThroughputAssertionAttribute : CounterMeasurementAttribute
    {
        public CounterThroughputAssertionAttribute(string counterName, MustBe condition, double averageOperationsPerSecond) : base(counterName)
        {
            Condition = condition;
            AverageOperationsPerSecond = averageOperationsPerSecond;
        }

        /// <summary>
        /// The test we're going to perform against the collected value of <see cref="CounterMeasurementAttribute.CounterName"/>
        /// and <see cref="AverageOperationsPerSecond"/>.
        /// </summary>
        public MustBe Condition { get; }

        /// <summary>
        /// The value that will be compared against the collected metric for <see cref="CounterMeasurementAttribute.CounterName"/>.
        /// </summary>
        public double AverageOperationsPerSecond { get; }

        /// <summary>
        /// Used only on <see cref="MustBe.Between"/> comparisons. This is the upper bound of that comparison
        /// and <see cref="AverageOperationsPerSecond"/> is the lower bound.
        /// </summary>
        public double? MaxAverageOperationsPerSecond { get; set; }
    }

    /// <summary>
    /// Performs an assertion against counters collected over the course of a benchmark.
    /// 
    /// This asserts the TOTAL AVERAGE NUMBER OF OPERATIONS values averaged over all runs of a benchmark.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CounterTotalAssertionAttribute : CounterMeasurementAttribute
    {
        public CounterTotalAssertionAttribute(string counterName, MustBe condition, double averageOperationsTotal) : base(counterName)
        {
            Condition = condition;
            AverageOperationsTotal = averageOperationsTotal;
        }

        /// <summary>
        /// The test we're going to perform against the collected value of <see cref="CounterMeasurementAttribute.CounterName"/>
        /// and <see cref="AverageOperationsTotal"/>.
        /// </summary>
        public MustBe Condition { get; }

        /// <summary>
        /// The value that will be compared against the collected metric for <see cref="CounterMeasurementAttribute.CounterName"/>.
        /// </summary>
        public double AverageOperationsTotal { get; }

        /// <summary>
        /// Used only on <see cref="MustBe.Between"/> comparisons. This is the upper bound of that comparison
        /// and <see cref="AverageOperationsTotal"/> is the lower bound.
        /// </summary>
        public double? MaxAverageOperationsTotal { get; set; }
    }
}

