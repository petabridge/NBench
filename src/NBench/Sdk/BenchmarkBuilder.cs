// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Collection;
using NBench.Collection.Memory;
using NBench.Metrics;
using NBench.Metrics.Counters;
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
            var counterSettings = Settings.CounterMeasurements.ToList();
            var counters = new List<Counter>(counterSettings.Count);

            // need to exclude counters first 
            var settingsExceptCounters = Settings.DistinctMeasurements.Except(counterSettings);
            foreach (var setting in settingsExceptCounters)
            {
                var selector = Settings.Collectors[setting.MetricName];
                var collector = selector.Create(Settings.RunMode, warmupData, setting);
                measurements.Add(new MeasureBucket(collector));
            }

            foreach (var counterSetting in counterSettings)
            {
                var setting = counterSetting;
                var selector = Settings.Collectors[setting.MetricName];
                var atomicCounter = new AtomicCounter();
                var createCounterBenchmark = new CreateCounterBenchmarkSetting(setting, atomicCounter);
                var collector = selector.Create(Settings.RunMode, warmupData, createCounterBenchmark);

                measurements.Add(new MeasureBucket(collector));
                counters.Add(new Counter(atomicCounter, setting.CounterName));

            }

            return new BenchmarkRun(measurements, counters, Settings.Trace);
        }
    }
}

