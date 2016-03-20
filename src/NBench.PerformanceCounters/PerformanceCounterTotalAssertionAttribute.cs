using System.Diagnostics;

namespace NBench.PerformanceCounters
{
    /// <summary>
    /// Performs an assertion against <see cref="PerformanceCounter"/> values collected over the course of a benchmark.
    /// 
    /// This asserts the TOTAL AVERAGE PERFORMANCE COUNTER VALUES over all runs of a benchmark.
    /// </summary>
    public class PerformanceCounterTotalAssertionAttribute : PerformanceCounterMeasurementAttribute
    {
        /// <summary>
        /// The test we're going to perform against the collected value of <see cref="CounterMeasurementAttribute.CounterName"/>
        /// and <see cref="AverageValueTotal"/>.
        /// </summary>
        public MustBe Condition { get; }

        /// <summary>
        /// The value that will be compared against the collected metric for <see cref="CounterMeasurementAttribute.CounterName"/>.
        /// </summary>
        public double AverageValueTotal { get; }

        /// <summary>
        /// Used only on <see cref="MustBe.Between"/> comparisons. This is the upper bound of that comparison
        /// and <see cref="AverageValueTotal"/> is the lower bound.
        /// </summary>
        public long? MaxAverageValueTotal { get; set; }

        public PerformanceCounterTotalAssertionAttribute(string categoryName, string counterName, MustBe condition, double averageValueTotal) : base(categoryName, counterName)
        {
            Condition = condition;
            AverageValueTotal = averageValueTotal;
        }
    }
}