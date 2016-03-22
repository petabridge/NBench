// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.Contracts;
using NBench.Metrics.GarbageCollection;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.Collection.GarbageCollection
{
    /// <summary>
    ///     Responsible for creating <see cref="MetricCollector" />s for monitoring GC metrics
    /// </summary>
    public class GcCollectionsSelector : MetricsCollectorSelector
    {
        public GcMetricName GcMetricName => (GcMetricName)Name;

        public GcCollectionsSelector() : this(MetricNames.GcCollections)
        {
        }

        public GcCollectionsSelector(GcMetricName name) : this(name, SysInfo.Instance)
        {
        }

        public GcCollectionsSelector(GcMetricName name, SysInfo systemInfo) : base(name, systemInfo)
        {
        }

        private MetricCollector CreateInstanceInternal(int gcGeneration)
        {
            return new GcCollectionsPerGenerationCollector(new GcMetricName(GcMetricName.Metric, (GcGeneration)gcGeneration), gcGeneration);
        }

        public override MetricCollector Create(RunMode runMode, WarmupData warmup,
            IBenchmarkSetting setting)
        {
            Contract.Assert(setting != null);
            Contract.Assert(setting is GcBenchmarkSetting);
            var gcSetting = setting as GcBenchmarkSetting;

            // ReSharper disable once PossibleNullReferenceException
            // covered by Code Contracts
            if (gcSetting.Generation == GcGeneration.AllGc)
            {
                throw new InvalidOperationException($"{gcSetting.Generation} is not supported by this collector");
            }

            return CreateInstanceInternal((int) gcSetting.Generation);
        }
    }
}

