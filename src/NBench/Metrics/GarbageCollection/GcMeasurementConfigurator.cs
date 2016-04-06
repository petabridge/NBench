// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NBench.Collection;
using NBench.Collection.GarbageCollection;
using NBench.Collection.Memory;
using NBench.Sdk;
using NBench.Sys;

namespace NBench.Metrics.GarbageCollection
{
    public class GcMeasurementConfigurator : MeasurementConfigurator<GcMeasurementAttribute>
    {
        public static GcCollectionsSelector Selector = new GcCollectionsSelector();

        public override MetricName GetName(GcMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            return new GcMetricName(instance.Metric, instance.Generation);
        }

        public override MetricsCollectorSelector GetMetricsProvider(GcMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            return Selector;
        }

        public override IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(GcMeasurementAttribute instance)
        {
            Contract.Requires(instance != null);
            var throughputAssertion = instance as GcThroughputAssertionAttribute;
            var totalAssertion = instance as GcTotalAssertionAttribute;


            var collectors = new List<IBenchmarkSetting>(SysInfo.Instance.MaxGcGeneration + 1);
            if (instance.Generation == GcGeneration.AllGc)
            {
                for (var i = 0; i <= SysInfo.Instance.MaxGcGeneration; i++)
                {
                    collectors.Add(CreateIndividualGcBenchmarkSetting((GcGeneration)i, instance, throughputAssertion, totalAssertion));
                }
            }
            else
            {
                collectors.Add(CreateIndividualGcBenchmarkSetting(instance.Generation, instance, throughputAssertion, totalAssertion));
            }
            return collectors;
        }

        private static IBenchmarkSetting CreateIndividualGcBenchmarkSetting(GcGeneration generation, GcMeasurementAttribute instance,
            GcThroughputAssertionAttribute throughputAssertion, GcTotalAssertionAttribute totalAssertion)
        {
            if (throughputAssertion != null)
            {
                return new GcBenchmarkSetting(instance.Metric, generation, AssertionType.Throughput,
                    new Assertion(throughputAssertion.Condition, throughputAssertion.AverageOperationsPerSecond,
                        throughputAssertion.MaxAverageOperationsPerSecond));
            }

            if (totalAssertion != null)
            {
                return new GcBenchmarkSetting(instance.Metric, generation, AssertionType.Total,
                    new Assertion(totalAssertion.Condition, totalAssertion.AverageOperationsTotal,
                        totalAssertion.MaxAverageOperationsTotal));
            }

            return new GcBenchmarkSetting(instance.Metric, generation, AssertionType.Total, Assertion.Empty);
        }
    }
}

