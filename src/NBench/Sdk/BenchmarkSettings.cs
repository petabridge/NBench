// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NBench.Collection;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Tracing;

namespace NBench.Sdk
{
    /// <summary>
    ///     Settings for how a particular <see cref="Benchmark" /> should be run and executed.
    /// </summary>
    public class BenchmarkSettings
    {
        internal class BenchmarkEqualityComparer : IEqualityComparer<IBenchmarkSetting>
        {
            public bool Equals(IBenchmarkSetting x, IBenchmarkSetting y)
            {
                /*
                 * We only care that the metrics have the same name
                 */
                return x.MetricName.Equals(y.MetricName);
            }

            public int GetHashCode(IBenchmarkSetting obj)
            {
                return obj.MetricName.GetHashCode();
            }
        }
        private static readonly BenchmarkEqualityComparer Comparer = new BenchmarkEqualityComparer();

        public const long DefaultRuntimeMilliseconds = 1000;

        public BenchmarkSettings(TestMode testMode, RunMode runMode, int numberOfIterations, int runTime,
            IEnumerable<IBenchmarkSetting> benchmarkSettings,
            IReadOnlyDictionary<MetricName, MetricsCollectorSelector> collectors)
            : this(
                testMode, runMode, numberOfIterations, runTime, 
                benchmarkSettings, collectors, string.Empty, string.Empty, NoOpBenchmarkTrace.Instance)
        {
        }

        public BenchmarkSettings(TestMode testMode, RunMode runMode, int numberOfIterations, int runTimeMilliseconds,
            IEnumerable<IBenchmarkSetting> benchmarkSettings,
            IReadOnlyDictionary<MetricName, MetricsCollectorSelector> collectors, string description, string skip, IBenchmarkTrace trace, bool concurrencyModeEnabled = false)
        {
            TestMode = testMode;
            RunMode = runMode;
            NumberOfIterations = numberOfIterations;
            RunTime = TimeSpan.FromMilliseconds(runTimeMilliseconds == 0 ? DefaultRuntimeMilliseconds : runTimeMilliseconds);
            Description = description;
            Skip = skip;

            // screen line for line duplicates that made it in by accident
            Measurements = new HashSet<IBenchmarkSetting>(benchmarkSettings).ToList();

            // now filter terms that measure the same quantities, but with different BenchmarkAssertions
            // because we only want to collect those measurements ONCE, but use them across mulitple BenchmarkAssertions.
            DistinctMeasurements = Measurements.Distinct(Comparer).ToList();

            Collectors = collectors;

            Trace = trace;
            ConcurrentMode = concurrencyModeEnabled;
        }

        /// <summary>
        /// </summary>
        public TestMode TestMode { get; private set; }

        /// <summary>
        ///     The mode in which the performance test will be executed.
        /// </summary>
        public RunMode RunMode { get; private set; }

        /// <summary>
        /// Indicates whether concurrency is enabled or not
        /// </summary>
        public bool ConcurrentMode { get; private set; }

        /// <summary>
        /// Indicates whether tracing is enabled or not
        /// </summary>
        public bool TracingEnabled => !(Trace is NoOpBenchmarkTrace);

        /// <summary>
        ///     Number of times this test will be run
        /// </summary>
        /// <remarks>Defaults to 10</remarks>
        public int NumberOfIterations { get; private set; }

        /// <summary>
        ///     Timeout the performance test and fail it if
        ///     any individual run exceeds this value.
        /// </summary>
        /// <remarks>Set to 0 to disable.</remarks>
        public TimeSpan RunTime { get; private set; }

        /// <summary>
        /// All of the configured metrics for this <see cref="Benchmark"/>
        /// </summary>
        public IReadOnlyList<IBenchmarkSetting> Measurements { get; private set; }

        /// <summary>
        /// If someone declares two measurements that measure the same thing, but carry different BenchmarkAssertions
        /// then those settings will only show up once on this list, whereas they might appear twice on <see cref="Measurements"/>
        /// </summary>
        public IReadOnlyList<IBenchmarkSetting> DistinctMeasurements { get; private set; }

        /// <summary>
        /// Counter settings, which require special treatment since they have to be injected
        /// into <see cref="BenchmarkContext"/>. Derived from <see cref="DistinctMeasurements"/>.
        /// </summary>
        public IEnumerable<CounterBenchmarkSetting> CounterMeasurements
            => DistinctMeasurements
                .Where(x => x is CounterBenchmarkSetting)
                .Cast<CounterBenchmarkSetting>();

        /// <summary>
        /// The table of collectors we're going to use to gather the metrics configured in <see cref="Measurements"/>
        /// </summary>
        public IReadOnlyDictionary<MetricName, MetricsCollectorSelector> Collectors { get; private set; }

       /// <summary>
       /// The <see cref="IBenchmarkTrace"/> implementation we will use for each <see cref="BenchmarkRun"/>
       /// </summary>
        public IBenchmarkTrace Trace { get; private set; }

        /// <summary>
        ///     Total number of all metrics tracked in this benchmark
        /// </summary>
        public int TotalTrackedMetrics => Measurements.Count;

        /// <summary>
        ///     A description of this performance benchmark, which will be written into the report.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        ///     If populated, this benchmark will be skipped and the skip reason will be written into the report.
        /// </summary>
        public string Skip { get; private set; }

        /// <summary>
        /// Indicates whether or not we will skip warmups for our benchmarks
        /// </summary>
        public bool SkipWarmups { get; set; }
    }
}