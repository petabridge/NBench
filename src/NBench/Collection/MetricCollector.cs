// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using NBench.Metrics;

namespace NBench.Collection
{
    /// <summary>
    /// Responsible for collecting metrics for various things inside NBench
    /// </summary>
    public abstract class MetricCollector : IDisposable
    {
        protected MetricCollector(MetricName name, string unitName)
        {
            Contract.Requires(name != null);
            Contract.Requires(!string.IsNullOrEmpty(unitName));
            Name = name;
            UnitName = unitName;
        }

        /// <summary>
        /// The name of this metric
        /// </summary>
        public MetricName Name { get; }

        /// <summary>
        /// The unit this metric is measured by
        /// </summary>
        public string UnitName { get; }

        /// <summary>
        /// Collects the value of this metric
        /// </summary>
        public abstract double Collect();

        public bool WasDisposed { get; private set; }

        /// <summary>
        /// Internal method for disposing any resources used by a specific <see cref="MetricCollector"/>
        /// implementation.
        /// </summary>
        protected virtual void DisposeInternal() { }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected void Dispose(bool dispose)
        {
            if (!dispose || WasDisposed) return;

            WasDisposed = true;
            DisposeInternal();
        }
    }
}

