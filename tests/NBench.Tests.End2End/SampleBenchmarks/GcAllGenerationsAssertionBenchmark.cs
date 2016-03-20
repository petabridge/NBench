namespace NBench.Tests.End2End.SampleBenchmarks
{
    public class GcAllGenerationsAssertionBenchmark
    {
        [PerfBenchmark(Description = "Verifier for GC.AllGenerations assertions", RunMode = RunMode.Iterations, TestMode = TestMode.Test,
            RunTimeMilliseconds = 1000, NumberOfIterations = 30)]
        [GcThroughputAssertion(GcMetric.TotalCollections, GcGeneration.AllGc, MustBe.ExactlyEqualTo, 0.0d)]
        public void ShouldNotGc()
        {
            // do nothing
        }
    }
}
