using System;
using System.Linq;
using NBench.Reporting;
using NBench.Sdk.Compiler;

namespace NBench.Sdk
{
    [Serializable]
    internal class BenchmarkRunnerResult
    {
        public bool AllTestsPassed { get; set; }
    }

    internal class BenchmarkRunner : MarshalByRefObject
    {
        private readonly IBenchmarkOutput _output;

        public BenchmarkRunner(IBenchmarkOutput output)
        {
            _output = output;
        }

        public BenchmarkRunnerResult Run(string testFile, string benchmarkName)
        {
            var discovery = new ReflectionDiscovery(_output);

            var assembly = AssemblyRuntimeLoader.LoadAssembly(testFile);
            var benchmarks = discovery.FindBenchmarks(assembly);

            var benchmark = benchmarks.FirstOrDefault(b => b.BenchmarkName == benchmarkName);
            benchmark.Run();
            benchmark.Finish();

            return new BenchmarkRunnerResult
            {
                AllTestsPassed = benchmark.AllAssertsPassed,
            };
        }
    }
}