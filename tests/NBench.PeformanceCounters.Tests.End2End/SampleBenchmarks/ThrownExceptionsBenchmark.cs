using System;
using NBench.PerformanceCounters;

namespace NBench.PeformanceCounters.Tests.End2End.SampleBenchmarks
{
    /// <summary>
    /// Track the number of exceptions thrown in a given benchmark
    /// </summary>
    public class ThrownExceptionsBenchmark
    {
        [PerfBenchmark(Description = "Track to see how many exceptions per second we can log using Windows Performance Counters", NumberOfIterations = 13, RunMode = RunMode.Throughput, TestMode = TestMode.Test, RunTimeMilliseconds = 200)]
        [PerformanceCounterMeasurement(".NET CLR Exceptions", "# of Exceps Thrown", InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "exceptions")]
        [PerformanceCounterThroughputAssertion(".NET CLR Exceptions", "# of Exceps Thrown", MustBe.GreaterThanOrEqualTo, 10.0d, InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "exceptions")]
        [PerformanceCounterTotalAssertion(".NET CLR Exceptions", "# of Exceps Thrown", MustBe.GreaterThanOrEqualTo, 10.0d, InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "exceptions")]
        public void ThrowingMethod()
        {
            try
            {
                throw new ApplicationException("WEEEEEEEEEE");
            }
            catch
            {
                // supress the error so NBench doesn't stop running the benchmark
            }
        }

        [PerfBenchmark(Description = "Track to ensure no exceptions are thrown Windows Performance Counters", NumberOfIterations = 13, RunMode = RunMode.Throughput, TestMode = TestMode.Test, RunTimeMilliseconds = 200)]
        [PerformanceCounterMeasurement(".NET CLR Exceptions", "# of Exceps Thrown", InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "exceptions")]
        [PerformanceCounterThroughputAssertion(".NET CLR Exceptions", "# of Exceps Thrown", MustBe.LessThanOrEqualTo, 10.0d, InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "exceptions")]
        [PerformanceCounterTotalAssertion(".NET CLR Exceptions", "# of Exceps Thrown", MustBe.LessThanOrEqualTo, 10.0d, InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "exceptions")]
        public void NoThrowingMethod()
        {
            // do nothing
        }
    }
}
