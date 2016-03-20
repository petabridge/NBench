// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using NBench.Metrics;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.Collection
{
    /// <summary>
    ///     Strategy pattern that's used to determine how to pick the appropriate
    ///     <see cref="MetricCollector" /> based on platform dependencies and user-preferences.
    /// </summary>
    public abstract class MetricsCollectorSelector
    {
        protected MetricsCollectorSelector(MetricName name) : this(name, SysInfo.Instance)
        {
        }

        protected MetricsCollectorSelector(MetricName name, SysInfo systemInfo)
        {
            Name = name;
            SystemInfo = systemInfo;
        }

        /// <summary>
        ///     The name of the underlying <see cref="MetricCollector" />. Will
        ///     be used in the counter regardless of the implementation selected.
        /// </summary>
        public MetricName Name { get; private set; }

        /// <summary>
        ///     Information about the current runtime - used in the course of making
        ///     decisions about tool selection and management
        /// </summary>
        public SysInfo SystemInfo { get; private set; }

        /// <summary>
        ///     Creates an instance for all applicable <see cref="MetricCollector" />s for this metric type.
        /// </summary>
        /// <param name="runMode">
        ///     The <see cref="RunMode" /> for this benchmark. Influences the type of
        ///     <see cref="MetricCollector" /> used in some instances.
        /// </param>
        /// <param name="setting">An implementation-specific <see cref="IBenchmarkSetting" /></param>
        /// <returns>A new <see cref="MetricCollector" /> instance. </returns>
        public MetricCollector Create(RunMode runMode, IBenchmarkSetting setting)
        {
            return Create(runMode, WarmupData.PreWarmup, setting);
        }

        /// <summary>
        ///     Creates an instance for all applicable <see cref="MetricCollector" />s for this metric type.
        /// </summary>
        /// <param name="runMode">
        ///     The <see cref="RunMode" /> for this benchmark. Influences the type of
        ///     <see cref="MetricCollector" /> used in some instances.
        /// </param>
        /// <param name="warmup">Warmup data. Influences the type of <see cref="MetricCollector" /> used in some instances.</param>
        /// <param name="setting">An implementation-specific <see cref="IBenchmarkSetting" /></param>
        /// <returns>A new <see cref="MetricCollector" /> instance.</returns>
        public abstract MetricCollector Create(RunMode runMode, WarmupData warmup,
            IBenchmarkSetting setting);
    }
}

