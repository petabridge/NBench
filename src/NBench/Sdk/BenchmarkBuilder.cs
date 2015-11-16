// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NBench.Collection;
using NBench.Collection.Memory;
using NBench.Metrics;
using NBench.Util;

namespace NBench.Sdk
{
    /// <summary>
    ///     Responsible for instrumenting all of the metrics
    ///     and producing the <see cref="BenchmarkContext" /> used inside each <see cref="BenchmarkRun" />
    /// </summary>
    public sealed class BenchmarkBuilder
    {
        /// <summary>
        ///     All built-in memory metric selectors
        /// </summary>
        public static readonly Dictionary<MemoryMetric, MetricsCollectorSelector> MemorySelectors = new Dictionary
            <MemoryMetric, MetricsCollectorSelector>
        {
            {
                MemoryMetric.TotalBytesAllocated, new TotalMemorySelector()
            }
        };

        public static readonly Dictionary<GcMetric, MetricsCollectorSelector> GcSelectors = new Dictionary
            <GcMetric, MetricsCollectorSelector>
        {
            {GcMetric.TotalCollections, new GcCollectionsSelector()}
        };

        public static readonly MetricsCollectorSelector CounterSelector = new CounterSelector();

        /// <summary>
        ///     Creates a new benchmark builder instance.
        /// </summary>
        /// <param name="settings">The settings compiled for this benchmark.</param>
        public BenchmarkBuilder(BenchmarkSettings settings)
        {
            Contract.Requires(settings.TotalTrackedMetrics > 0);
            Settings = settings;
        }

        public BenchmarkSettings Settings { get; }

        /// <summary>
        ///     Generates a new <see cref="BenchmarkRun" /> based on the provided settings, available system metrics,
        ///     and (optionally) the duration of the last run.
        /// </summary>
        /// <param name="warmupData">Data collected during warm-up</param>
        /// <returns>A new <see cref="BenchmarkRun" /> instance.</returns>
        public BenchmarkRun NewRun(WarmupData warmupData)
        {
            var numberOfMetrics = Settings.TotalTrackedMetrics;
            var measurements = new List<MeasureBucket>(numberOfMetrics);
            var counters = new List<Counter>(Settings.CounterBenchmarks.Count);

            for (var i = 0; i < Settings.DistinctMemoryBenchmarks.Count; i++)
            {
                var setting = Settings.DistinctMemoryBenchmarks[i];
                var collectors = MemorySelectors[setting.Metric].Create(Settings.RunMode, warmupData, setting);
                foreach (var collector in collectors)
                    measurements.Add(new MeasureBucket(collector, warmupData.NumberOfObservedSamples));
            }

            for (var i = 0; i < Settings.DistinctGcBenchmarks.Count; i++)
            {
                var setting = Settings.DistinctGcBenchmarks[i];
                var collectors = GcSelectors[setting.Metric].Create(Settings.RunMode, warmupData, setting);
                foreach (var collector in collectors)
                    measurements.Add(new MeasureBucket(collector, warmupData.NumberOfObservedSamples));
            }

            for (var i = 0; i < Settings.DistinctCounterBenchmarks.Count; i++)
            {
                var setting = Settings.DistinctCounterBenchmarks[i];
                var atomicCounter = new AtomicCounter();
                var createCounterBenchmark = new CreateCounterBenchmarkSetting(setting, atomicCounter);
                var collectors = CounterSelector.Create(Settings.RunMode, warmupData, createCounterBenchmark);
                foreach (var collector in collectors)
                {
                    measurements.Add(new MeasureBucket(collector, warmupData.NumberOfObservedSamples));
                    counters.Add(new Counter(atomicCounter, setting.CounterName));
                }
            }

            return new BenchmarkRun(measurements, counters);
        }
    }
}

