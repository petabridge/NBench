// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using NBench.Reporting;

namespace NBench.Sdk.Compiler
{
    /// <summary>
    ///     <see cref="IDiscovery" /> implementation built using reflection
    /// </summary>
    public class ReflectionDiscovery : IDiscovery
    {
        public static readonly Type PerformanceBenchmarkAttributeType = typeof (PerfBenchmarkAttribute);
        public static readonly Type MeasurementAttributeType = typeof (MeasurementAttribute);
        public static readonly Type BenchmarkContextType = typeof (BenchmarkContext);

        public ReflectionDiscovery(IBenchmarkOutput output)
        {
            Output = output;
        }

        public IBenchmarkOutput Output { get; }

        //TODO: https://github.com/petabridge/NBench/issues/9
        public IEnumerable<Benchmark> FindBenchmarks(Assembly targetAssembly)
        {
            var benchmarkMetaData = ClassesWithPerformanceBenchmarks(targetAssembly).SelectMany(CreateBenchmarksForClass).ToList();
            var benchmarks = new List<Benchmark>(benchmarkMetaData.Count());
            foreach (var data in benchmarkMetaData)
            {
                var invoker = CreateInvokerForBenchmark(data);
                var settings = CreateSettingsForBenchmark(data);
                benchmarks.Add(new Benchmark(settings, invoker, Output));
            }
            return benchmarks;
        }

        public IEnumerable<Benchmark> FindBenchmarks(Type targetType)
        {
            var benchmarkMetaData = CreateBenchmarksForClass(targetType.GetTypeInfo()).ToList();
            var benchmarks = new List<Benchmark>(benchmarkMetaData.Count());
            foreach (var data in benchmarkMetaData)
            {
                var invoker = CreateInvokerForBenchmark(data);
                var settings = CreateSettingsForBenchmark(data);
                benchmarks.Add(new Benchmark(settings, invoker, Output));
            }
            return benchmarks;
        }

        public static BenchmarkSettings CreateSettingsForBenchmark(BenchmarkClassMetadata benchmarkClass)
        {
            var allBenchmarkMethodAttributes = benchmarkClass.Run.InvocationMethod.GetCustomAttributes().ToList();

            var performanceTestAttribute =
                allBenchmarkMethodAttributes.Single(a => a is PerfBenchmarkAttribute) as
                    PerfBenchmarkAttribute;
            Contract.Assert(performanceTestAttribute != null);

            var memorySettings =
                allBenchmarkMethodAttributes.Where(a => a is MemoryMeasurementAttribute)
                    .Cast<MemoryMeasurementAttribute>()
                    .Select(CreateBenchmarkSetting)
                    .ToList();
            var counterBenchmarkSettings =
                allBenchmarkMethodAttributes.Where(a => a is CounterMeasurementAttribute)
                    .Cast<CounterMeasurementAttribute>()
                    .Select(CreateBenchmarkSetting)
                    .ToList();
            var gcBenchmarkSettings =
                allBenchmarkMethodAttributes.Where(a => a is GcMeasurementAttribute)
                    .Cast<GcMeasurementAttribute>()
                    .Select(CreateBenchmarkSetting)
                    .ToList();

            return new BenchmarkSettings(performanceTestAttribute.TestMode, performanceTestAttribute.RunMode,
                performanceTestAttribute.NumberOfIterations, performanceTestAttribute.RunTimeMilliseconds,
                gcBenchmarkSettings, memorySettings, counterBenchmarkSettings, performanceTestAttribute.Description,
                performanceTestAttribute.Skip);
        }

        public static IBenchmarkInvoker CreateInvokerForBenchmark(BenchmarkClassMetadata benchmarkClass)
        {
            return new ReflectionBenchmarkInvoker(benchmarkClass);
        }

        public static MemoryBenchmarkSetting CreateBenchmarkSetting(
            MemoryMeasurementAttribute memoryMeasurement)
        {
            var assertion = memoryMeasurement as MemoryAssertionAttribute;
            if (assertion != null)
            {
                return new MemoryBenchmarkSetting(memoryMeasurement.Metric,
                    new Assertion(assertion.Condition, assertion.AverageBytes, assertion.MaxAverageBytes));
            }
            return new MemoryBenchmarkSetting(memoryMeasurement.Metric, Assertion.Empty);
        }

        public static GcBenchmarkSetting CreateBenchmarkSetting(GcMeasurementAttribute gcMeasurement)
        {
            var throughputAssertion = gcMeasurement as GcThroughputAssertionAttribute;
            var totalAssertion = gcMeasurement as GcTotalAssertionAttribute;
            if (throughputAssertion != null)
            {
                return new GcBenchmarkSetting(gcMeasurement.Metric, gcMeasurement.Generation, AssertionType.Throughput, new Assertion(throughputAssertion.Condition, throughputAssertion.AverageOperationsPerSecond, throughputAssertion.MaxAverageOperationsPerSecond));
            }
            if (totalAssertion != null)
            {
                return new GcBenchmarkSetting(gcMeasurement.Metric, gcMeasurement.Generation, AssertionType.Total, new Assertion(totalAssertion.Condition, totalAssertion.AverageOperationsTotal, totalAssertion.MaxAverageOperationsTotal));
            }
            
            return new GcBenchmarkSetting(gcMeasurement.Metric, gcMeasurement.Generation, AssertionType.Total, Assertion.Empty);
        }

        public static CounterBenchmarkSetting CreateBenchmarkSetting(CounterMeasurementAttribute counterMeasurement)
        {
            var throughputAssertion = counterMeasurement as CounterThroughputAssertionAttribute;
            var totalAssertion = counterMeasurement as CounterTotalAssertionAttribute;
            if (throughputAssertion != null)
            {
                return new CounterBenchmarkSetting(counterMeasurement.CounterName, AssertionType.Throughput, new Assertion(throughputAssertion.Condition, throughputAssertion.AverageOperationsPerSecond, throughputAssertion.MaxAverageOperationsPerSecond));
            }
            if (totalAssertion != null)
            {
                return new CounterBenchmarkSetting(counterMeasurement.CounterName, AssertionType.Total, new Assertion(totalAssertion.Condition, totalAssertion.AverageOperationsTotal, totalAssertion.MaxAverageOperationsTotal));
            }

            return new CounterBenchmarkSetting(counterMeasurement.CounterName, AssertionType.Total, Assertion.Empty);
        }

        /// <summary>
        ///     Finds all classes with at least one method decorated with a <see cref="PerfBenchmarkAttribute" />.
        ///     inside <see cref="targetAssembly" />.
        /// </summary>
        /// <param name="targetAssembly">The assembly we're scanning for benchmarks.</param>
        /// <returns>
        ///     A list of all applicable types that contain at least one method with a
        ///     <see cref="PerfBenchmarkAttribute" />.
        /// </returns>
        public static IReadOnlyList<TypeInfo> ClassesWithPerformanceBenchmarks(Assembly targetAssembly)
        {
            Contract.Requires(targetAssembly != null);
            
            return targetAssembly.DefinedTypes.Where(
                x =>
                    x.DeclaredMethods.Any(MethodHasValidBenchmark)).ToList();
        }

        public static IReadOnlyList<BenchmarkClassMetadata> CreateBenchmarksForClass(Type classWithBenchmarks)
        {
            return CreateBenchmarksForClass(classWithBenchmarks.GetTypeInfo());   
        }

        public static IReadOnlyList<BenchmarkClassMetadata> CreateBenchmarksForClass(TypeInfo classWithBenchmarks)
        {
            Contract.Requires(classWithBenchmarks != null);
            var setupMethod = GetSetupMethod(classWithBenchmarks);
            var cleanupMethod = GetCleanupMethod(classWithBenchmarks);

            var allPerfMethods =
                classWithBenchmarks.DeclaredMethods.Where(
                    MethodHasValidBenchmark).ToList();

            var benchmarks = new List<BenchmarkClassMetadata>(allPerfMethods.Count);
            foreach (var perfMethod in allPerfMethods)
            {
                var takesContext = MethodTakesBenchmarkContext(perfMethod);
                benchmarks.Add(new BenchmarkClassMetadata(classWithBenchmarks.AsType(), setupMethod,
                    new BenchmarkMethodMetadata(perfMethod, takesContext, false), cleanupMethod));
            }
            return benchmarks;
        }

        private static bool MethodHasValidBenchmark(MethodInfo x)
        {
            return x.GetCustomAttributes(PerformanceBenchmarkAttributeType, true).Any()
                        && x.GetCustomAttributes(MeasurementAttributeType, true).Any();
        }

        public static BenchmarkMethodMetadata GetSetupMethod(TypeInfo classWithBenchmarks)
        {
            Contract.Requires(classWithBenchmarks != null);
            if (
                !classWithBenchmarks.DeclaredMethods.Any(
                    y => y.GetCustomAttributes(typeof (PerfSetupAttribute), true).Any()))
                return BenchmarkMethodMetadata.Empty;

            //TODO: https://github.com/petabridge/NBench/issues/10
            var matchingMethod =
                classWithBenchmarks.DeclaredMethods.Single(
                    x => x.GetCustomAttributes(typeof (PerfSetupAttribute), true).Any());

            var takesContext = MethodTakesBenchmarkContext(matchingMethod);
            return new BenchmarkMethodMetadata(matchingMethod, takesContext, false);
        }

        public static BenchmarkMethodMetadata GetCleanupMethod(TypeInfo classWithBenchmarks)
        {
            Contract.Requires(classWithBenchmarks != null);
            if (
                !classWithBenchmarks.DeclaredMethods.Any(
                    y => y.GetCustomAttributes(typeof (PerfCleanupAttribute), true).Any()))
                return BenchmarkMethodMetadata.Empty;

            //TODO: https://github.com/petabridge/NBench/issues/10
            var matchingMethod =
                classWithBenchmarks.DeclaredMethods.Single(
                    x => x.GetCustomAttributes(typeof (PerfCleanupAttribute), true).Any());

            var takesContext = MethodTakesBenchmarkContext(matchingMethod);
            return new BenchmarkMethodMetadata(matchingMethod, takesContext, false);
        }

        public static bool MethodTakesBenchmarkContext(MethodInfo info)
        {
            Contract.Requires(info != null);
            return info.GetParameters().Any(x => x.ParameterType == BenchmarkContextType);
        }
    }

    /// <summary>
    ///     Metadata used to create a <see cref="Benchmark" />
    /// </summary>
    public struct BenchmarkClassMetadata
    {
        public BenchmarkClassMetadata(Type benchmarkClass, BenchmarkMethodMetadata setup, BenchmarkMethodMetadata run,
            BenchmarkMethodMetadata cleanup)
        {
            BenchmarkClass = benchmarkClass;
            Setup = setup;
            Run = run;
            Cleanup = cleanup;
        }

        public Type BenchmarkClass { get; private set; }
        public BenchmarkMethodMetadata Setup { get; }
        public BenchmarkMethodMetadata Run { get; }
        public BenchmarkMethodMetadata Cleanup { get; }
    }

    /// <summary>
    ///     Metadata used to indicate how a single method works
    /// </summary>
    public struct BenchmarkMethodMetadata
    {
        /// <summary>
        ///     Empty method that won't be called during a benchmark run
        /// </summary>
        public static readonly BenchmarkMethodMetadata Empty = new BenchmarkMethodMetadata(null, false, true);

        public BenchmarkMethodMetadata(MethodInfo invocationMethod, bool takesBenchmarkContext, bool skip)
        {
            InvocationMethod = invocationMethod;
            TakesBenchmarkContext = takesBenchmarkContext;
            Skip = skip;
        }

        /// <summary>
        ///     The method we'll invoke for cleanup / teardown
        /// </summary>
        public MethodInfo InvocationMethod { get; }

        /// <summary>
        ///     Does this method take <see cref="BenchmarkContext" />?
        /// </summary>
        public bool TakesBenchmarkContext { get; private set; }

        /// <summary>
        ///     Skip this method
        /// </summary>
        public bool Skip { get; private set; }
    }
}

