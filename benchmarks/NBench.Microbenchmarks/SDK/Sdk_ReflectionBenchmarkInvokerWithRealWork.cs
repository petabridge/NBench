// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using NBench.Sdk;
using NBench.Util;
using static NBench.Sdk.Compiler.ReflectionDiscovery;

namespace NBench.Microbenchmarks.SDK
{
    [LegacyJitX64Job]
    [LegacyJitX86Job]
    [RyuJitX64Job]
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

        [Benchmark(Description = "How quickly can we invoke when injecting context into a ReflectionBenchmarkInvoker")]
        public void InvokeRunWithContext()
        {
            _contextInvoker.InvokeRun(BenchmarkContext.Empty);
        }
    }
}

