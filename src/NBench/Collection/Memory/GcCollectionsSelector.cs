// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NBench.Metrics;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.Collection.Memory
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

        public override IEnumerable<MetricCollector> Create(RunType runType, WarmupData warmup,
            IBenchmarkSetting setting)
        {
            Contract.Assert(setting != null);
            Contract.Assert(setting is GcBenchmarkSetting);
            var gcSetting = setting as GcBenchmarkSetting;

            // ReSharper disable once PossibleNullReferenceException
            // covered by Code Contracts
            if (gcSetting.Generation == GcGeneration.AllGc)
            {
                var collectors = new List<MetricCollector>(SystemInfo.MaxGcGeneration + 1);
                for (var i = 0; i <= SystemInfo.MaxGcGeneration; i++)
                {
                    collectors.Add(CreateInstanceInternal(i));
                }
                return collectors;
            }

            return new[] {CreateInstanceInternal((int) gcSetting.Generation)};
        }
    }
}

