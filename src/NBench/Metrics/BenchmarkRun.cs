// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Metrics.Counters;
using NBench.Reporting;

namespace NBench.Metrics
{
    /// <summary>
    ///     Used to collect metrics for a given instance of a benchmark
    /// </summary>
    public class BenchmarkRun : IDisposable
    {
        public BenchmarkRun(IReadOnlyList<MeasureBucket> measures, IReadOnlyList<Counter> counters, IBenchmarkTrace trace)
        {
            Contract.Requires(measures != null);
            Contract.Requires(counters != null);
            Measures = measures;
            MeasureCount = measures.Count;
            Counters = counters.ToDictionary(key => key.Name, v => v);
            Context = new BenchmarkContext(Counters, trace);
        }

        public bool WasDisposed { get; private set; }
        public int MeasureCount { get; }
        public IReadOnlyList<MeasureBucket> Measures { get; }
        public IReadOnlyDictionary<CounterMetricName, Counter> Counters { get; }
        private readonly List<Exception> _exceptions = new List<Exception>();
        public IReadOnlyList<Exception> Exceptions => _exceptions;

        /// <summary>
        /// Returns <c>true</c> if any <see cref="Exception"/>s were thrown during this run.
        /// </summary>
        public bool IsFaulted => Exceptions.Count > 0;

        public BenchmarkContext Context { get; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        ///     Sample all actively used benchmarks in this run
        /// </summary>
        public void Sample(long elapsedTicks)
        {
            for (var i = 0; i < MeasureCount; i++)
                Measures[i].Collect(elapsedTicks);
        }

        /// <summary>
        /// Adds an <see cref="Exception"/> to this <see cref="BenchmarkRun"/>.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> thrown while running the benchmark.</param>
        public void WithException(Exception ex)
        {
            _exceptions.Add(ex);
        }

        /// <summary>
        /// Collect a final report for this <see cref="BenchmarkRun"/>
        /// </summary>
        /// <returns>A compiled report for all of the underlying <see cref="MeasureBucket"/>s.</returns>
        public BenchmarkRunReport ToReport(TimeSpan elapsedTime)
        {
            return new BenchmarkRunReport(elapsedTime, Measures.Select(x => x.ToReport()), Exceptions);
        }

        private void Dispose(bool isDisposing)
        {
            if (!isDisposing || WasDisposed) return;

            WasDisposed = true;
            for (var i = 0; i < MeasureCount; i++)
                Measures[i].Dispose(); //free the underlying collector
        }
    }
}

