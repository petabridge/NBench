// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Reporting;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using Xunit;

namespace NBench.Tests.Sdk.Compiler
{
    public class ReflectionDiscoverySpecs
    {
        public interface IInterfaceBenchmark
        {
            [PerfSetup]
            void Setup(BenchmarkContext context);

            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = 100, RunTimeMilliseconds = 1000)]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
            void Run1();

            [PerfCleanup]
            void Cleanup();
        }

        public class ConreteBenchmarkImpl : IInterfaceBenchmark {
            public void Setup(BenchmarkContext context)
            {
                
            }

            public void Run1()
            {
                
            }

            public void Cleanup()
            {
                
            }
        }

        public abstract class AbstractBenchmark
        {
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {

            }

            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = 100, RunTimeMilliseconds = 1000)]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
            public void Run1()
            {
                ConcreteBenchmarkMethod();
            }

            protected abstract void ConcreteBenchmarkMethod();

            [PerfCleanup]
            public void Cleanup()
            {
            }
        }

        public class InheritedBenchmark : AbstractBenchmark {
            protected override void ConcreteBenchmarkMethod()
            {
                
            }
        }

        public class DefaultMemoryMeasurementBenchmark
        {
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {

            }

            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = 100, RunTimeMilliseconds = 1000)]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
            public void Run1()
            {

            }

            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = 100, RunTimeMilliseconds = 1000)]
            [CounterMeasurement("MyOtherCounter")]
            [CounterThroughputAssertion("MyCounter", MustBe.GreaterThan, 100.0d)]
            public void Run2()
            {

            }

            [PerfCleanup]
            public void Cleanup()
            {
            }
        }

        public class SimpleBenchmark
        {
            public void Setup(BenchmarkContext context)
            {

            }

            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = 100, RunTimeMilliseconds = 1000)]
            [CounterMeasurement("MyCounter")]
            [CounterThroughputAssertion("MyCounter", MustBe.GreaterThan, 100.0d)]
            public void Run()
            {

            }

            public void Cleanup()
            {
            }
        }

        public class BenchmarkWithoutMeasurements
        {
            /// <summary>
            /// Should be a no-op
            /// </summary>
            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = 100, RunTimeMilliseconds = 1000)]
            public void Run()
            {

            }
        }

        public class BenchmarkWithOnlySkippedMethods
        {
            /// <summary>
            /// Should be skipped
            /// </summary>
            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = 100, RunTimeMilliseconds = 1000, Skip = "SKIP")]
            [CounterMeasurement("MyCounter")]
            [CounterThroughputAssertion("MyCounter", MustBe.GreaterThan, 100.0d)]
            public void Run()
            {

            }
        }

        public static readonly TypeInfo ComplexBenchmarkTypeInfo =
            typeof (DefaultMemoryMeasurementBenchmark).GetTypeInfo();

        public static readonly TypeInfo SimpleBenchmarkTypeInfo = typeof (SimpleBenchmark).GetTypeInfo();

        public static readonly TypeInfo BenchmarkWithoutMeasurementsTypeInfo =
            typeof (BenchmarkWithoutMeasurements).GetTypeInfo();

        public static readonly TypeInfo SkippedBenchmarksTypeInfo =
            typeof (BenchmarkWithOnlySkippedMethods).GetTypeInfo();

        [Fact]
        public void ShouldFindSetupMethod()
        {
            var setupMethod = ReflectionDiscovery.GetSetupMethod(ComplexBenchmarkTypeInfo);
            Assert.NotNull(setupMethod.InvocationMethod);
            Assert.False(setupMethod.Skip);
        }

        [Fact]
        public void ShouldNotFindSetupMethod()
        {
            var setupMethod = ReflectionDiscovery.GetSetupMethod(SimpleBenchmarkTypeInfo);
            Assert.Null(setupMethod.InvocationMethod);
            Assert.True(setupMethod.Skip);
        }

        [Fact]
        public void ShouldDetermineIfMethodUsesContext()
        {
            var setupMethod = ReflectionDiscovery.GetSetupMethod(ComplexBenchmarkTypeInfo);
            var usesContext = ReflectionDiscovery.MethodTakesBenchmarkContext(setupMethod.InvocationMethod);
            Assert.True(usesContext);
        }

        [Fact]
        public void ShouldFindCleanupMethod()
        {
            var cleanupMethod = ReflectionDiscovery.GetCleanupMethod(ComplexBenchmarkTypeInfo);
            Assert.NotNull(cleanupMethod.InvocationMethod);
            Assert.False(cleanupMethod.Skip);
        }

        [Fact]
        public void ShouldNotFindCleanupMethod()
        {
            var cleanupMethod = ReflectionDiscovery.GetCleanupMethod(SimpleBenchmarkTypeInfo);
            Assert.Null(cleanupMethod.InvocationMethod);
            Assert.True(cleanupMethod.Skip);
        }

        [Fact]
        public void ShouldFindMultipleBenchmarkMethods()
        {
            var benchmarkMethods = ReflectionDiscovery.CreateBenchmarksForClass(ComplexBenchmarkTypeInfo);
            Assert.Equal(2, benchmarkMethods.Count);
            Assert.True(benchmarkMethods.All(x => !x.Setup.Skip));
            Assert.True(benchmarkMethods.All(x => !x.Cleanup.Skip));
        }

        [Fact]
        public void ShouldFindSingleBenchmarkMethod()
        {
            var benchmarkMethods = ReflectionDiscovery.CreateBenchmarksForClass(SimpleBenchmarkTypeInfo);
            Assert.Equal(1, benchmarkMethods.Count);
            Assert.True(benchmarkMethods.All(x => x.Setup.Skip));
            Assert.True(benchmarkMethods.All(x => x.Cleanup.Skip));
        }

        [Fact]
        public void ShouldProduceBenchmarkSettings_Complex()
        {
            var discovery = new ReflectionDiscovery(NoOpBenchmarkOutput.Instance);
            var benchmarkMetaData = ReflectionDiscovery.CreateBenchmarksForClass(ComplexBenchmarkTypeInfo);
            var benchmarkSettings = discovery.CreateSettingsForBenchmark(benchmarkMetaData.First());

            Assert.Equal(TestMode.Test, benchmarkSettings.TestMode);
            Assert.Equal(PerfBenchmarkAttribute.DefaultRunType, benchmarkSettings.RunMode);
            Assert.Equal(0, benchmarkSettings.Measurements.Count(x => x is GcBenchmarkSetting));
            Assert.Equal(2, benchmarkSettings.Measurements.Count(x => x is MemoryBenchmarkSetting));
            Assert.Equal(1, benchmarkSettings.DistinctMeasurements.Count(x => x is MemoryBenchmarkSetting));
            Assert.Equal(0, benchmarkSettings.CounterMeasurements.Count());
        }

        [Fact]
        public void ShouldNotCreateBenchmarkForClassWithNoDeclaredMeasurements()
        {
            var benchmarkMetaData = ReflectionDiscovery.CreateBenchmarksForClass(BenchmarkWithoutMeasurementsTypeInfo);
            Assert.Equal(0, benchmarkMetaData.Count);
        }

        [Fact]
        public void ShouldNotCreateBenchmarkForClassWithOnlySkippedBenchmarkMethods()
        {
            var benchmarkMetaData = ReflectionDiscovery.CreateBenchmarksForClass(SkippedBenchmarksTypeInfo);
            Assert.Equal(0, benchmarkMetaData.Count);
        }

        [Theory]
        [InlineData(typeof(AbstractBenchmark), 0, false, false)]
        [InlineData(typeof(IInterfaceBenchmark), 0, false, false)]
        [InlineData(typeof(InheritedBenchmark), 1, true, true)]
        //[InlineData(typeof(ConreteBenchmarkImpl), 1, true, true)] // Unsupported case right now
        public void ShouldCreateBenchmark(Type benchmarkType, int numberOfBenchmarks, bool hasSetup, bool hasCleanup)
        {
            var typeInfo = benchmarkType.GetTypeInfo();
            var setupMethod = ReflectionDiscovery.GetSetupMethod(typeInfo);
            var cleanupMethod = ReflectionDiscovery.GetCleanupMethod(typeInfo);
            var benchmarkMetaData = ReflectionDiscovery.CreateBenchmarksForClass(typeInfo);
            Assert.Equal(!hasSetup, setupMethod.Skip);
            Assert.Equal(!hasCleanup, cleanupMethod.Skip);
            Assert.Equal(numberOfBenchmarks, benchmarkMetaData.Count);
        }

    }
}

