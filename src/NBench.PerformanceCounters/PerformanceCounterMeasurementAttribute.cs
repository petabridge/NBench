using System;
using System.Diagnostics;

namespace NBench.PerformanceCounters
{
    /// <summary>
    /// Use these to tell NBench to grab special values, such as the name of the current process,
    /// at run-time for use in specific <see cref="PerformanceCounterMeasurementAttribute"/>s.
    /// </summary>
    public static class NBenchPerformanceCounterConstants
    {
        /// <summary>
        /// Signals to NBench to substitute <see cref="Process.ProcessName"/> 
        /// for the <see cref="PerformanceCounterMeasurementAttribute.InstanceName"/>.
        /// </summary>
        public const string CurrentProcessName = "CURRENT_PROCESS_NAME";
    }

    /// <summary>
    /// Used to instrument a <see cref="PerfBenchmarkAttribute"/> method with a <see cref="PerformanceCounter"/>.
    /// 
    /// All performance counters instrumented by this method are read-only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PerformanceCounterMeasurementAttribute : MeasurementAttribute
    {
        public PerformanceCounterMeasurementAttribute(string categoryName, string counterName)
        {
            CategoryName = categoryName;
            CounterName = counterName;
        }

        /// <summary>
        /// The name of the performance counter category with which this performance counter is associated.
        /// </summary>
        public string CategoryName { get; private set; }

        /// <summary>
        /// The name of the performance counter.
        /// </summary>
        public string CounterName { get; private set; }

        /// <summary>
        /// The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Human-readable name of the measurement units associated with this performance counter.
        /// 
        /// Used solely for reporting purposes - designed to make it easier to understand what the metric reads.
        /// For instance, if this performance counter was measuring disk writes the unit name would be "bytes"
        /// Or if this performance counter were measuring page faults, the unit name would be "page faults"
        /// </summary>
        public string UnitName { get; set; }
    }
}
