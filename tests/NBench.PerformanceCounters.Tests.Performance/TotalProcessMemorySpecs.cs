namespace NBench.PerformanceCounters.Tests.Performance
{
    public class TotalProcessMemorySpecs
    {
        [PerfBenchmark(Description = "Capture the total amount of process memory currently in use", 
            TestMode = TestMode.Measurement, RunMode = RunMode.Iterations, NumberOfIterations = 13)]
        [PerformanceCounterMeasurement("Memory", "% Committed Bytes In Use", UnitName = "bytes")]
        [PerformanceCounterMeasurement(".NET CLR Memory", "# Total committed Bytes", InstanceName = NBenchPerformanceCounterConstants.CurrentProcessName, UnitName = "bytes")]
        public void TotalProcessMemoryInUse()
        {
            // useless work that allocates memory
            var data = new byte[ByteConstants.SixtyFourKb];
        }
    }
}
