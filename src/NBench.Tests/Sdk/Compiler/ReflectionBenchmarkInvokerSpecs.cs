// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        #region Synchronous benchmark classes (no TPL)

        public class BenchmarkWithContext
        {
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {
                ReflectionBenchmarkInvokerSpecs.SetupContextSet = context != null;
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run(BenchmarkContext context)
            {
                ReflectionBenchmarkInvokerSpecs.RunContextSet = context != null;
            }

            [PerfCleanup]
            public void Cleanup(BenchmarkContext context)
            {
                ReflectionBenchmarkInvokerSpecs.CleanupContextSet = context != null;
            }
        }

        public class BenchmarkWithoutContext
        {
            [PerfSetup]
            public void Setup()
            {
                ReflectionBenchmarkInvokerSpecs.SetupContextSet = true;
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                ReflectionBenchmarkInvokerSpecs.RunContextSet = true;
            }

            [PerfCleanup]
            public void Cleanup()
            {
                ReflectionBenchmarkInvokerSpecs.CleanupContextSet = true;
            }
        }

        public class BenchmarkWithoutSetup
        {

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                ReflectionBenchmarkInvokerSpecs.RunContextSet = true;
            }

            [PerfCleanup]
            public void Cleanup()
            {
                ReflectionBenchmarkInvokerSpecs.CleanupContextSet = true;
            }
        }

        public class BenchmarkWithoutCleanup
        {
            [PerfSetup]
            public void Setup()
            {
                ReflectionBenchmarkInvokerSpecs.SetupContextSet = true;
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                ReflectionBenchmarkInvokerSpecs.RunContextSet = true;
            }
        }

        public class BenchmarkWithRunOnly
        {
            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                ReflectionBenchmarkInvokerSpecs.RunContextSet = true;
            }
        }

        #endregion

        #region Benchmarks with TPL Tasks

        /// <summary>
        /// Hogs the thread for a second before moving on
        /// </summary>
        private static readonly Action RealWorkMethod = () =>
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
        };

        /// <summary>
        /// Doesn't do anything
        /// </summary>
        private static readonly Action NoWorkMethod = () => { };

        /// <summary>
        /// Blocks on <see cref="Setup"/> and <see cref="Cleanup"/>
        /// </summary>
        public class BenchmarkWithContextAndRealAsyncWork
        {
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {
                Task.Run(RealWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.SetupContextSet = context != null;
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run(BenchmarkContext context)
            {
                RunContextSet = SetupContextSet && !CleanupContextSet;
            }

            [PerfCleanup]
            public void Cleanup(BenchmarkContext context)
            {
                Task.Run(RealWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.CleanupContextSet = context != null;
            }
        }

        /// <summary>
        /// Blocks on <see cref="Setup"/> and <see cref="Cleanup"/>
        /// </summary>
        public class BenchmarkWithContextAndNoAsyncWork
        {
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {
                Task.Run(NoWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.SetupContextSet = context != null;
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run(BenchmarkContext context)
            {
                RunContextSet = SetupContextSet && !CleanupContextSet;
            }

            [PerfCleanup]
            public void Cleanup(BenchmarkContext context)
            {
                Task.Run(NoWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.CleanupContextSet = context != null;
            }
        }

        /// <summary>
        /// Blocks on <see cref="Setup"/> and <see cref="Cleanup"/>
        /// </summary>
        public class BenchmarkWithoutContextAndRealAsyncWork
        {
            [PerfSetup]
            public void Setup()
            {
                Task.Run(RealWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.SetupContextSet = true;
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                RunContextSet = SetupContextSet && !CleanupContextSet;
            }

            [PerfCleanup]
            public void Cleanup()
            {
                Task.Run(RealWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.CleanupContextSet = true;
            }
        }

        /// <summary>
        /// Blocks on <see cref="Setup"/> and <see cref="Cleanup"/>
        /// </summary>
        public class BenchmarkWithoutContextAndNoAsyncWork
        {
            [PerfSetup]
            public void Setup()
            {
                Task.Run(NoWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.SetupContextSet = true;
            }

            [PerfBenchmark]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            public void Run()
            {
                RunContextSet = SetupContextSet && !CleanupContextSet;
            }

            [PerfCleanup]
            public void Cleanup()
            {
                Task.Run(NoWorkMethod).Wait();
                ReflectionBenchmarkInvokerSpecs.CleanupContextSet = true;
            }
        }

        #endregion

        [Theory]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithContext), true, true, true)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithoutContext), true, true, true)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithoutSetup), false, true, true)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithoutCleanup), true, true, false)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithRunOnly), false, true, false)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithContextAndRealAsyncWork), true, true, true)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithContextAndNoAsyncWork), true, true, true)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithoutContextAndRealAsyncWork), true, true, true)]
        [InlineData(typeof(ReflectionBenchmarkInvokerSpecs.BenchmarkWithoutContextAndNoAsyncWork), true, true, true)]
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

