using System.Linq;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using Xunit;

namespace NBench.PeformanceCounters.Tests.End2End
{
    public class PerformanceCounterIntegrationTests
    {
        private static readonly IBenchmarkOutput _benchmarkOutput = new ActionBenchmarkOutput(report => { }, results =>
        {
            foreach (var assertion in results.AssertionResults)
            {
                Assert.True(assertion.Passed, results.BenchmarkName + " " + assertion.Message);
            }
        });

        private readonly IDiscovery _discovery = new ReflectionDiscovery(_benchmarkOutput);

        [Fact]
        public void ShouldPassAllBenchmarks()
        {
            var benchmarks = _discovery.FindBenchmarks(GetType().Assembly).ToList();
            Assert.True(benchmarks.Count >= 1);
            Benchmark.PrepareForRun(); // force some GC here
            for (var i = 0; i < benchmarks.Count; i++)
            {
                Benchmark.PrepareForRun(); // force some GC here
                benchmarks[i].Run();
                benchmarks[i].Finish();
            }
        }
    }
}
