// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NBench.Sdk
{
    /// <summary>
    ///     Settings for how a particular <see cref="Benchmark" /> should be run and executed.
    /// </summary>
    public class BenchmarkSettings
    {
        public const long DefaultRuntimeMilliseconds = 1000;

        public BenchmarkSettings(TestMode testMode, RunMode runMode, int numberOfIterations, int runTime,
            IEnumerable<GcBenchmarkSetting> gcBenchmarks,
            IEnumerable<MemoryBenchmarkSetting> memoryBenchmarks,
            IEnumerable<CounterBenchmarkSetting> counterBenchmarks)
            : this(
                testMode, runMode, numberOfIterations, runTime, gcBenchmarks, memoryBenchmarks,
                counterBenchmarks, string.Empty, string.Empty)
        {
        }

        public BenchmarkSettings(TestMode testMode, RunMode runMode, int numberOfIterations, int runTimeMilliseconds,
            IEnumerable<GcBenchmarkSetting> gcBenchmarks,
            IEnumerable<MemoryBenchmarkSetting> memoryBenchmarks,
            IEnumerable<CounterBenchmarkSetting> counterBenchmarks, string description, string skip)
        {
            TestMode = testMode;
            RunMode = runMode;
            NumberOfIterations = numberOfIterations;
            RunTime = TimeSpan.FromMilliseconds(runTimeMilliseconds == 0 ? DefaultRuntimeMilliseconds : runTimeMilliseconds);
            Description = description;
            Skip = skip;

            // Strip out any duplicates here
            // TODO: is it better to move that responsibility outside of the BenchmarkSettings class?

            GcBenchmarks = new HashSet<GcBenchmarkSetting>(gcBenchmarks).ToList();
            MemoryBenchmarks = new HashSet<MemoryBenchmarkSetting>(memoryBenchmarks).ToList();
            CounterBenchmarks = new HashSet<CounterBenchmarkSetting>(counterBenchmarks).ToList();

            DistinctGcBenchmarks =
                GcBenchmarks.Distinct(GcBenchmarkSetting.GcBenchmarkDistinctComparer.Instance).ToList();

            DistinctCounterBenchmarks =
                CounterBenchmarks.Distinct(CounterBenchmarkSetting.CounterBenchmarkDistinctComparer.Instance).ToList();

            DistinctMemoryBenchmarks =
                MemoryBenchmarks.Distinct(MemoryBenchmarkSetting.MemoryBenchmarkDistinctComparer.Instance).ToList();
        }

        /// <summary>
        ///     The mode in which this performance test assertions will be tested.
        /// </summary>
        public TestMode TestMode { get; private set; }

        /// <summary>
        ///     The mode in which the performance test will be executed.
        /// </summary>
        public RunMode RunMode { get; private set; }

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

        public IReadOnlyList<GcBenchmarkSetting> GcBenchmarks { get; }
        public IReadOnlyList<MemoryBenchmarkSetting> MemoryBenchmarks { get; }
        public IReadOnlyList<CounterBenchmarkSetting> CounterBenchmarks { get; }
        internal IReadOnlyList<GcBenchmarkSetting> DistinctGcBenchmarks { get; }
        internal IReadOnlyList<MemoryBenchmarkSetting> DistinctMemoryBenchmarks { get; }
        internal IReadOnlyList<CounterBenchmarkSetting> DistinctCounterBenchmarks { get; }

        /// <summary>
        ///     Total number of all metrics tracked in this benchmark
        /// </summary>
        public int TotalTrackedMetrics => GcBenchmarks.Count + MemoryBenchmarks.Count + CounterBenchmarks.Count;

        /// <summary>
        ///     A description of this performance benchmark, which will be written into the report.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        ///     If populated, this benchmark will be skipped and the skip reason will be written into the report.
        /// </summary>
        public string Skip { get; private set; }
    }
}