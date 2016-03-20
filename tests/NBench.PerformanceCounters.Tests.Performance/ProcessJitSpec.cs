using System;
using System.Linq;

namespace NBench.PerformanceCounters.Tests.Performance
{
    public class ProcessJitSpec
    {
        [PerfBenchmark(Description = "Capture the amount of .NET CLR JIT compiler time being used",
            TestMode = TestMode.Measurement, RunMode = RunMode.Iterations, NumberOfIterations = 13)]
        [PerformanceCounterMeasurement(".NET CLR Jit", "% Time in Jit", InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "% Time in Jit")]
        [PerformanceCounterMeasurement(".NET CLR Jit", "# of Methods Jitted", InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "methods")]
        public void TimeInJit()
        {
            Func<int, bool> isEven = i => i%2 == 0;
            foreach (var i in Enumerable.Range(0, 10000))
                isEven(i);
        }
    }
}
