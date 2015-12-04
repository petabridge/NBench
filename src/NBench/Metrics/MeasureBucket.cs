// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Collection;
using NBench.Reporting;

namespace NBench.Metrics
{
    /// <summary>
    /// A metric that's collected over the course of a test run using
    /// a stack of values sampled over time, the deltas of which will later be calculated.
    /// </summary>
    public class MeasureBucket : IDisposable
    {
        protected readonly MetricMeasurement[] Measurements;
        protected int CurrentCount = 0;
        private const int MaxMeasures = 2;

        private MetricCollector _collector;

        public MeasureBucket(MetricCollector collector)
        {
            Contract.Requires(collector != null);
            Name = collector.Name;
            Unit = collector.UnitName;
            _collector = collector;
            Measurements = new MetricMeasurement[MaxMeasures];
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

        public void Collect(long elapsedTicks)
        {
            if (!WasDisposed)
            {
                Measurements[CurrentCount] = new MetricMeasurement(elapsedTicks, _collector.Collect());
                CurrentCount = CurrentCount + 1 < MaxMeasures ?  CurrentCount + 1 : CurrentCount; //WRAP
            }
                
        }

        /// <summary>
        /// Converts the data collected within this <see cref="MeasureBucket"/>
        /// into a <see cref="MetricRunReport"/>.
        /// </summary>
        /// <returns>A metric run report containing all of the delta for this measure bucket.</returns>
        public MetricRunReport ToReport()
        {
            var front = Measurements.First();
            var last = Measurements.Last();
            return new MetricRunReport(Name, Unit, last.MetricValue - front.MetricValue, last.ElapsedTicks);
        }
 

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

