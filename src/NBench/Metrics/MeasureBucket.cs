// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Collection;
using NBench.Reporting;
using NBench.Util;

namespace NBench.Metrics
{
    /// <summary>
    /// A metric that's collected over the course of a test run using
    /// a stack of values sampled over time, the deltas of which will later be calculated.
    /// </summary>
    public class MeasureBucket : IDisposable
    {
        protected readonly Queue<MetricMeasurement> Measurements;

        private MetricCollector _collector;

        public MeasureBucket(MetricCollector collector, int initialSize)
        {
            Contract.Requires(initialSize >= 0);
            Contract.Requires(collector != null);
            Name = collector.Name;
            Unit = collector.UnitName;
            _collector = collector;
            Measurements = new Queue<MetricMeasurement>(initialSize);
        }

        public bool WasDisposed { get; private set; }

        /// <summary>
        /// Name of the metric
        /// </summary>
        public MetricName Name { get; }

        /// <summary>
        /// Name of the unit
        /// </summary>
        public string Unit { get; }

        public void Collect(TimeSpan elapsedNanos)
        {
            if(!WasDisposed)
                Measurements.Enqueue(new MetricMeasurement(elapsedNanos, _collector.Collect()));
        }

        /// <summary>
        /// Converts the data collected within this <see cref="MeasureBucket"/>
        /// into a <see cref="MetricRunReport"/>.
        /// </summary>
        /// <returns>A metric run report containing all of the <see cref="RawValues"/> for this measure bucket.</returns>
        public MetricRunReport ToReport()
        {
            return new MetricRunReport(Name, Unit, RawValues);
        }
        
        /// <summary>
        /// All of the raw, uncalculated values
        /// </summary>
        public IDictionary<TimeSpan, long> RawValues
            => Measurements.ToDictionary(key => key.Elapsed, value => value.MetricValue);

        /// <summary>
        /// Returns a sorted list of tuples with the following data:
        /// * Elapsed Nanoseconds / Delta from Start
        /// </summary>
        public IDictionary<TimeSpan,double> Deltas => Measurements.DistanceFromStart();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (!isDisposing || WasDisposed) return;

            WasDisposed = true;
            _collector.Dispose();
            _collector = null;
        }
    }
}

