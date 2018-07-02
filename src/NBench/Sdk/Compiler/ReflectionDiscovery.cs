// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using NBench.Collection;
using NBench.Metrics;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Tracing;

namespace NBench.Sdk.Compiler
{
    /// <summary>
    ///     <see cref="IDiscovery" /> implementation built using reflection
    /// </summary>
    public partial class ReflectionDiscovery : IDiscovery
    {
        public static readonly Type PerformanceBenchmarkAttributeType = typeof (PerfBenchmarkAttribute);
        public static readonly Type MeasurementAttributeType = typeof (MeasurementAttribute);
        public static readonly Type BenchmarkContextType = typeof (BenchmarkContext);

        public ReflectionDiscovery(IBenchmarkOutput output) : this(output, DefaultBenchmarkAssertionRunner.Instance, new RunnerSettings(){ TracingEnabled = false})
        {
        }


        public ReflectionDiscovery(IBenchmarkOutput output, IBenchmarkAssertionRunner benchmarkAssertions, RunnerSettings settings)
        {
            Output = output;
            BenchmarkAssertions = benchmarkAssertions;
            RunnerSettings = settings;
            if(RunnerSettings.TracingEnabled)
                Trace = new BenchmarkOutputTrace(Output);
            else
                Trace = NoOpBenchmarkTrace.Instance;
        }

        /// <summary>
        /// WARNING: SHARED MUTABLE STATE BETWEEN REFLECTIONDISCOVERY INSTANCES
        /// 
        /// Bit of a hack - used internally by <see cref="ReflectionDiscovery"/> for
        /// logging compilation warnings, which are static methods.
        /// 
        /// Writes to the console by default, but can be overridden through the constructor.
        /// </summary>
        internal static IBenchmarkOutput ReflectionOutput = new ConsoleBenchmarkOutput();

        public IBenchmarkOutput Output { get; }
        public IBenchmarkAssertionRunner BenchmarkAssertions { get; }
        public RunnerSettings RunnerSettings { get; }
        public IBenchmarkTrace Trace { get; }

        public IEnumerable<Benchmark> FindBenchmarks(Assembly targetAssembly)
        {
            var benchmarkMetaData = ClassesWithPerformanceBenchmarks(targetAssembly).SelectMany(CreateBenchmarksForClass).ToList();
            var benchmarks = new List<Benchmark>(benchmarkMetaData.Count());
            foreach (var data in benchmarkMetaData)
            {
                var invoker = CreateInvokerForBenchmark(data);
                var settings = CreateSettingsForBenchmark(data);
                benchmarks.Add(new Benchmark(settings, invoker, Output, BenchmarkAssertions));
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
                benchmarks.Add(new Benchmark(settings, invoker, Output, BenchmarkAssertions));
            }
            return benchmarks;
        }

        public BenchmarkSettings CreateSettingsForBenchmark(BenchmarkClassMetadata benchmarkClass)
        {
            var allBenchmarkMethodAttributes = benchmarkClass.Run.InvocationMethod.GetCustomAttributes().ToList();

            var performanceTestAttribute =
                allBenchmarkMethodAttributes.Single(a => a is PerfBenchmarkAttribute) as
                    PerfBenchmarkAttribute;
            Contract.Assert(performanceTestAttribute != null);

            var allMeasurementAttributes =
                allBenchmarkMethodAttributes.Where(a => MeasurementAttributeType.IsInstanceOfType(a)).Cast<MeasurementAttribute>();

            var measurements = new List<IBenchmarkSetting>();
            var collectors = new Dictionary<MetricName, MetricsCollectorSelector>();

            foreach (var measurement in allMeasurementAttributes)
            {
                var configurator = GetConfiguratorForMeasurement(measurement.GetType());
                if (configurator is MeasurementConfigurator.EmptyConfigurator)
                {
                    Output.Warning($"Unable to find valid configurator for {measurement} - skipping...");
                    continue;
                }

                var benchmarkSettings = configurator.GetBenchmarkSettings(measurement);
                var selector = configurator.GetMetricsProvider(measurement);
                foreach (var setting in benchmarkSettings)
                {
                    var name = setting.MetricName;
                    measurements.Add(setting);
                    collectors[name] = selector;
                }
            }

            // TODO: need to start packing more of these settings in as propreties rather than constructor arguments
            // it's becoming unsustainable, the number of different things we need to pass in here
            return new BenchmarkSettings(performanceTestAttribute.TestMode, performanceTestAttribute.RunMode,
                performanceTestAttribute.NumberOfIterations, performanceTestAttribute.RunTimeMilliseconds,
                measurements, collectors, performanceTestAttribute.Description,
                performanceTestAttribute.Skip, Trace, RunnerSettings.ConcurrentModeEnabled){ SkipWarmups = performanceTestAttribute.SkipWarmups };
        }

        public static IBenchmarkInvoker CreateInvokerForBenchmark(BenchmarkClassMetadata benchmarkClass)
        {
            return new ReflectionBenchmarkInvoker(benchmarkClass);
        }

        /// <summary>
        ///     Finds all classes with at least one method decorated with a <see cref="PerfBenchmarkAttribute" />.
        ///     inside <paramref name="targetAssembly" />.
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
                    x.GetMethods().Any(MethodHasValidBenchmark) && !x.IsAbstract && x.IsClass).ToList();
        }

        public static IReadOnlyList<BenchmarkClassMetadata> CreateBenchmarksForClass(Type classWithBenchmarks)
        {
            return CreateBenchmarksForClass(classWithBenchmarks.GetTypeInfo());   
        }

        public static IReadOnlyList<BenchmarkClassMetadata> CreateBenchmarksForClass(TypeInfo classWithBenchmarks)
        {
            Contract.Requires(classWithBenchmarks != null);

            if(IsTypeInvalidForBenchmarks(classWithBenchmarks))
                return new List<BenchmarkClassMetadata>();

            var setupMethod = GetSetupMethod(classWithBenchmarks);
            var cleanupMethod = GetCleanupMethod(classWithBenchmarks);

            var allPerfMethods =
                classWithBenchmarks.GetMethods().Where(
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
            var hasPerformanceBenchmarkAttribute = x.IsDefined(PerformanceBenchmarkAttributeType, true);
            var hasAtLeastOneMeasurementAttribute = x.IsDefined(MeasurementAttributeType, true);
            var skipReason = (x.GetCustomAttribute(PerformanceBenchmarkAttributeType) as
                                                        PerfBenchmarkAttribute)?.Skip;
            var benchmarkIsSkipped = hasPerformanceBenchmarkAttribute &&
                                    !String.IsNullOrEmpty(skipReason);

            // code below is for adding interface support
            //var bla =
            //   (from @interface in x.DeclaringType.GetInterfaces()
            //    let map = x.DeclaringType.GetInterfaceMap(@interface)
            //    let index = Array.IndexOf(map.TargetMethods, x)
            //    where index >= 0
            //    select map.InterfaceMethods[index]).FirstOrDefault();
            //var hasPerformanceBenchmarkAttributeOnInterface = bla.IsDefined(PerformanceBenchmarkAttributeType), true);


            /*
             * If user defined a PerformanceBenchmark attribute but never added on any Measurement
             * attributes, then we need to log a warning here.
             */
            if (hasPerformanceBenchmarkAttribute && !hasAtLeastOneMeasurementAttribute)
            {
                ReflectionOutput.Warning($"{x.DeclaringType?.Name}+{x.Name} has a declared PerformanceBenchmarkAttribute but no declared measurements. Skipping...");
            }
            else if (benchmarkIsSkipped)
            {
                ReflectionOutput.WriteLine($"Skipping {x.DeclaringType?.Name}+{x.Name}. Reason: {skipReason}.");
            }

            return hasPerformanceBenchmarkAttribute && hasAtLeastOneMeasurementAttribute && !benchmarkIsSkipped;
        }

        public static BenchmarkMethodMetadata GetSetupMethod(TypeInfo classWithBenchmarks)
        {
            Contract.Requires(classWithBenchmarks != null);
            var setupMethods = classWithBenchmarks.GetMethods().Where(
                y => y.IsDefined(typeof (PerfSetupAttribute), true)).ToList();
            if (!setupMethods.Any() || IsTypeInvalidForBenchmarks(classWithBenchmarks))
                return BenchmarkMethodMetadata.Empty;

            // Need to log and throw an error here for benchmarks that have multiple setups declared
            if (setupMethods.Count > 1)
            {
                var ex =
                    new NBenchException(
                        $"{classWithBenchmarks.Name} has a declared {setupMethods.Count} PerfSetupAttributes. A maximum of 1 is allowed per class. Failing...");
                ReflectionOutput.Error(ex.Message);
                throw ex;
            }

            var matchingMethod = setupMethods.Single();

            var takesContext = MethodTakesBenchmarkContext(matchingMethod);
            return new BenchmarkMethodMetadata(matchingMethod, takesContext, false);
        }

        public static BenchmarkMethodMetadata GetCleanupMethod(TypeInfo classWithBenchmarks)
        {
            Contract.Requires(classWithBenchmarks != null);
            var cleanupMethods = classWithBenchmarks.GetMethods().Where(
               y => y.IsDefined(typeof(PerfCleanupAttribute), true)).ToList();
            if (!cleanupMethods.Any() || IsTypeInvalidForBenchmarks(classWithBenchmarks))
                return BenchmarkMethodMetadata.Empty;

            // Need to log and throw an error here for benchmarks that have multiple setups declared
            if (cleanupMethods.Count > 1)
            {
                var ex =
                    new NBenchException(
                        $"{classWithBenchmarks.Name} has a declared {cleanupMethods.Count} PerfCleanupAttributes. A maximum of 1 is allowed per class. Failing...");
                ReflectionOutput.Error(ex.Message);
                throw ex;
            }

            var matchingMethod = cleanupMethods.Single();

            var takesContext = MethodTakesBenchmarkContext(matchingMethod);
            return new BenchmarkMethodMetadata(matchingMethod, takesContext, false);
        }

        public static bool IsTypeInvalidForBenchmarks(TypeInfo info)
        {
            Contract.Requires(info != null);
            return info.IsAbstract || info.IsInterface;
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

