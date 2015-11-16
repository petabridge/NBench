// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Xunit;
using Xunit.Sdk;
using static NBench.Sdk.Compiler.ReflectionDiscovery;

namespace NBench.Tests.Sdk.Compiler
{
    public class ReflectionBenchmarkInvokerSpecs
    {
        public ReflectionBenchmarkInvokerSpecs()
        {
            SetupContextSet = false;
            RunContextSet = false;
            CleanupContextSet = false;
        }

        public static bool SetupContextSet;
        public static bool RunContextSet;
        public static bool CleanupContextSet;

        public class BenchmarkWithContext
        {
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {
                SetupContextSet = context != null;
            }

            [PerformanceBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run(BenchmarkContext context)
            {
                RunContextSet = context != null;
            }

            [PerfCleanup]
            public void Cleanup(BenchmarkContext context)
            {
                CleanupContextSet = context != null;
            }
        }

        public class BenchmarkWithoutContext
        {
            [PerfSetup]
            public void Setup()
            {
                SetupContextSet = true;
            }

            [PerformanceBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                RunContextSet = true;
            }

            [PerfCleanup]
            public void Cleanup()
            {
                CleanupContextSet = true;
            }
        }

        public class BenchmarkWithoutSetup
        {

            [PerformanceBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                RunContextSet = true;
            }

            [PerfCleanup]
            public void Cleanup()
            {
                CleanupContextSet = true;
            }
        }

        public class BenchmarkWithoutCleanup
        {
            [PerfSetup]
            public void Setup()
            {
                SetupContextSet = true;
            }

            [PerformanceBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                RunContextSet = true;
            }
        }

        public class BenchmarkWithRunOnly
        {
            [PerformanceBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                RunContextSet = true;
            }
        }

        [Theory]
        [InlineData(typeof(BenchmarkWithContext), true, true, true)]
        [InlineData(typeof(BenchmarkWithoutContext), true, true, true)]
        [InlineData(typeof(BenchmarkWithoutSetup), false, true, true)]
        [InlineData(typeof(BenchmarkWithoutCleanup), true, true, false)]
        [InlineData(typeof(BenchmarkWithRunOnly), false, true, false)]
        public void ShouldInvokeAllMethodsCorrectly(Type benchmarkType, bool setupHit, bool runHit, bool cleanupHit)
        {
            AssertAllContextFalse();
            var benchmarks = CreateBenchmarksForClass(benchmarkType);
            var invoker = CreateInvokerForBenchmark(benchmarks.Single());
            invoker.InvokePerfSetup(BenchmarkContext.Empty);
            invoker.InvokeRun(BenchmarkContext.Empty);
            invoker.InvokePerfCleanup(BenchmarkContext.Empty);
            Assert.Equal(setupHit, SetupContextSet);
            Assert.Equal(runHit, RunContextSet);
            Assert.Equal(cleanupHit, CleanupContextSet);
        }

        public void AssertAllContextFalse()
        {
            Assert.False(SetupContextSet);
            Assert.False(CleanupContextSet);
            Assert.False(RunContextSet);
        }
    }
}

