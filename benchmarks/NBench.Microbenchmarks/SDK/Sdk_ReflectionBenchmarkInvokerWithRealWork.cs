using System.Linq;
using BenchmarkDotNet.Tasks;
using NBench.Sdk;
using NBench.Util;
using static NBench.Sdk.Compiler.ReflectionDiscovery;

namespace NBench.Microbenchmarks.SDK
{
    [BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.LegacyJit)]
    [BenchmarkTask(platform: BenchmarkPlatform.X86, jitVersion: BenchmarkJitVersion.LegacyJit)]
    [BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.RyuJit)]
    public class Sdk_ReflectionBenchmarkInvokerWithRealWork
    {
        public class AtomicCounterBenchmark
        {
            private readonly AtomicCounter _counter = new AtomicCounter();

            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {

            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run(BenchmarkContext context)
            {
                _counter.Increment();
            }

            [PerfCleanup]
            public void Cleanup(BenchmarkContext context)
            {

            }
        }

        private readonly IBenchmarkInvoker _contextInvoker;
        public Sdk_ReflectionBenchmarkInvokerWithRealWork()
        {
            var benchmarkType = typeof(AtomicCounterBenchmark);
            var benchmarks = CreateBenchmarksForClass(benchmarkType);
            _contextInvoker = CreateInvokerForBenchmark(benchmarks.Single());
            _contextInvoker.InvokePerfSetup(BenchmarkContext.Empty);
        }

        [BenchmarkDotNet.Benchmark("How quickly can we invoke when injecting context into a ReflectionBenchmarkInvoker")]
        public void InvokeRunWithContext()
        {
            _contextInvoker.InvokeRun(BenchmarkContext.Empty);
        }
    }
}
