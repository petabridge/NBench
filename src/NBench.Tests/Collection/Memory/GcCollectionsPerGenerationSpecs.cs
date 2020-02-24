// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Linq;
using NBench.Collection.Memory;
using NBench.Metrics;
using NBench.Metrics.GarbageCollection;
using NBench.Sdk;
using NBench.Sys;
using Xunit;

namespace NBench.Tests.Collection.Memory
{
    public class GcCollectionsPerGenerationSpecs
    {
        /// <summary>
        /// Designed to help us avoid N-1 errors when counting the number of available GC generations on a given machine
        /// </summary>
        [Fact]
        public void GcCollectionsSelector_should_create_1_GcCollectionsPerGenerationCollector_per_Generation()
        {
            var gcCollectionsSelector = new GcMeasurementConfigurator();
            var gcGenerations = SysInfo.Instance.MaxGcGeneration;
            var gcSetting = new GcMeasurementAttribute(GcMetric.TotalCollections, GcGeneration.AllGc);
            var gcCollectors = gcCollectionsSelector.GetBenchmarkSettings(gcSetting).Cast<GcBenchmarkSetting>().ToList();
            Assert.Equal(gcGenerations + 1,gcCollectors.Count);
            Assert.Equal(gcGenerations, (int)gcCollectors.Max(x => x.Generation));
        }
    }
}

