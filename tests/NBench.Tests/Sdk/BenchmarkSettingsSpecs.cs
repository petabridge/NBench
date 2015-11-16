// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Linq;
using NBench.Sdk;
using Xunit;

namespace NBench.Tests.Sdk
{
    public class BenchmarkSettingsSpecs
    {
        [Fact]
        public void ShouldFilterOutDuplicateGcSettings()
        {
            var gcSettings = new[]
            {
                new GcBenchmarkSetting(GcMetric.TotalCollections, GcGeneration.Gen0, AssertionType.Throughput,
                    Assertion.Empty),
                new GcBenchmarkSetting(GcMetric.TotalCollections, GcGeneration.Gen0, AssertionType.Total,
                    Assertion.Empty),
                new GcBenchmarkSetting(GcMetric.TotalCollections, GcGeneration.Gen1, AssertionType.Throughput,
                Assertion.Empty),
                new GcBenchmarkSetting(GcMetric.TotalCollections, GcGeneration.Gen1, AssertionType.Total,
                    Assertion.Empty)
            };

            var distinctGcSettings = gcSettings.Distinct(GcBenchmarkSetting.GcBenchmarkDistinctComparer.Instance);
            //  according to normal equality, all 4 should be distinct as they have different assertion settings
            Assert.Equal(4, gcSettings.Distinct().Count());

            // but using our special EqualityComparer<T>, they really track only 2 distinct metrics
            Assert.Equal(2, distinctGcSettings.Count());
        }

        [Fact]
        public void ShouldFilterOutDuplicateMemorySettings()
        {
            var memorySettings = new[]
            {
                new MemoryBenchmarkSetting(MemoryMetric.TotalBytesAllocated,
                    Assertion.Empty),
                new MemoryBenchmarkSetting(MemoryMetric.TotalBytesAllocated,
                    new Assertion(MustBe.ExactlyEqualTo, 1.0d, null)),
            };

            var distinctMemorySettings = memorySettings.Distinct(MemoryBenchmarkSetting.MemoryBenchmarkDistinctComparer.Instance);
            Assert.Equal(2, memorySettings.Length);
            Assert.Equal(1, distinctMemorySettings.Count());
        }

        [Fact]
        public void ShouldFilterOutDuplicateCounterSettings()
        {
            var counterSettings = new[]
            {
                new CounterBenchmarkSetting("counter1", AssertionType.Throughput, Assertion.Empty),
                new CounterBenchmarkSetting("counter1", AssertionType.Total, Assertion.Empty),
                new CounterBenchmarkSetting("counter2", AssertionType.Throughput, Assertion.Empty),
                new CounterBenchmarkSetting("counter2", AssertionType.Total, Assertion.Empty),
            };

            var distinctMemorySettings = counterSettings.Distinct(CounterBenchmarkSetting.CounterBenchmarkDistinctComparer.Instance).ToList();
            Assert.Equal(4, counterSettings.Length);
            Assert.Equal(2, distinctMemorySettings.Count());
            Assert.Equal(1, distinctMemorySettings.Count(x => x.CounterName.CounterName.Equals("counter1")));
            Assert.Equal(1, distinctMemorySettings.Count(x => x.CounterName.CounterName.Equals("counter2")));
        }
    }
}

