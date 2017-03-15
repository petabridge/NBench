// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using NBench.Sdk;
using static NBench.Sdk.Compiler.ReflectionDiscovery;

namespace NBench.Microbenchmarks.SDK
{
    [LegacyJitX64Job]
    [LegacyJitX86Job]
    [RyuJitX64Job]
    public class Sdk_ReflectionBenchmarkInvoker
    {
        public class BenchmarkWithContext
        {
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {
                
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run(BenchmarkContext context)
            {
                
            }

            [PerfCleanup]
            public void Cleanup(BenchmarkContext context)
            {
                
            }
        }

        public class BenchmarkWithoutContext
        {
            [PerfSetup]
            public void Setup()
            {

            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {

            }

            [PerfCleanup]
            public void Cleanup()
            {

            }
        }

        private readonly IBenchmarkInvoker _contextInvoker;
        private readonly IBenchmarkInvoker _withoutContextInvoker;

        public Sdk_ReflectionBenchmarkInvoker()
        {
            var benchmarkType = typeof (BenchmarkWithContext);
            var benchmarks = CreateBenchmarksForClass(benchmarkType);
            _contextInvoker = CreateInvokerForBenchmark(benchmarks.Single());
            _contextInvoker.InvokePerfSetup(BenchmarkContext.Empty);

            benchmarkType = typeof(BenchmarkWithoutContext);
            benchmarks = CreateBenchmarksForClass(benchmarkType);
            _withoutContextInvoker = CreateInvokerForBenchmark(benchmarks.Single());
            _withoutContextInvoker.InvokePerfSetup(BenchmarkContext.Empty);
        }

        [Benchmark(Description = "How quickly can we invoke when injecting context into a ReflectionBenchmarkInvoker")]
        public void InvokeRunWithContext()
        {
            _contextInvoker.InvokeRun(BenchmarkContext.Empty);
        }

        [Benchmark(Description = "How quickly can we invoke when NOT injecting context into a ReflectionBenchmarkInvoker")]
        public void InvokeRunWithoutContext()
        {
            _withoutContextInvoker.InvokeRun(BenchmarkContext.Empty);
        }
    }
}

