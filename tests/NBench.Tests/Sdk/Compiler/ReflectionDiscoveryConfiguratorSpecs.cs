// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using NBench.Collection;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Reporting;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using Xunit;

namespace NBench.Tests.Sdk.Compiler
{
    /// <summary>
    /// Specs used to determine how accurately <see cref="ReflectionDiscovery"/>
    /// can find the correct <see cref="IMeasurementConfigurator{T}"/> instances
    /// that correspond to a given <see cref="MeasurementAttribute"/>
    /// </summary>
    public class ReflectionDiscoveryConfiguratorSpecs
    {
        #region Internal test classes

        private class SimpleMeasurementAttribute : MeasurementAttribute { }

        private class DerivedMeasurementAttribute : SimpleMeasurementAttribute { }

        /// <summary>
        /// Used to test assumptions about type discovery and assignability
        /// </summary>
        private class SimpleMeasurementConfigurator : MeasurementConfigurator<SimpleMeasurementAttribute> {
            public override MetricName GetName(SimpleMeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }

            public override MetricsCollectorSelector GetMetricsProvider(SimpleMeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(SimpleMeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }
        }

        private class DerivedMeasurementConfigurator : MeasurementConfigurator<DerivedMeasurementAttribute> {
            public override MetricName GetName(DerivedMeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }

            public override MetricsCollectorSelector GetMetricsProvider(DerivedMeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(DerivedMeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Used to test "no match" cases
        /// </summary>
        private class UnsupportedMeasurementAttribute : MeasurementAttribute { }

        #endregion

        [Theory]
        [InlineData(typeof(MemoryMeasurementAttribute), typeof(MemoryMeasurementConfigurator))]
        [InlineData(typeof(MemoryAssertionAttribute), typeof(MemoryMeasurementConfigurator))]
        [InlineData(typeof(SimpleMeasurementAttribute), typeof(SimpleMeasurementConfigurator))]
        [InlineData(typeof(DerivedMeasurementAttribute), typeof(DerivedMeasurementConfigurator))]
        public void ReflectionDiscoveryCanFindBestFittingConfiguratorViaReflection(Type measurementType, Type expectedConfiguratorType)
        {
            var allConfigurators = ReflectionDiscovery.LoadAllTypeConfigurators();
            var actualMatch = ReflectionDiscovery.FindBestMatchingConfiguratorForMeasurement(measurementType, allConfigurators);
            Assert.True(expectedConfiguratorType == actualMatch);
        }

        [Theory]
        [InlineData(typeof(MemoryMeasurementAttribute), typeof(MemoryMeasurementConfigurator),true)]
        [InlineData(typeof(MemoryAssertionAttribute), typeof(MemoryMeasurementConfigurator), false)] //MemoryMeasurementConfigurator supports MeasurementAssertionAttribute, but isn't an EXACT match
        [InlineData(typeof(MemoryMeasurementAttribute), typeof(SimpleMeasurementConfigurator), false)]
        [InlineData(typeof(MemoryAssertionAttribute), typeof(SimpleMeasurementConfigurator), false)]
        [InlineData(typeof(SimpleMeasurementAttribute), typeof(SimpleMeasurementConfigurator), true)]
        [InlineData(typeof(DerivedMeasurementAttribute), typeof(DerivedMeasurementConfigurator), true)]
        [InlineData(typeof(DerivedMeasurementAttribute), typeof(SimpleMeasurementConfigurator), false)]
        public void ReflectionDiscoveryCanMatchExactConcreteTypes(Type measurementType, Type expectedConfiguratorType, bool matchResult)
        {
            Assert.True(ReflectionDiscovery.ConfiguratorSupportsMeasurement(measurementType, expectedConfiguratorType, true) == matchResult, $"Expected {matchResult} but got ${!matchResult}");
        }

        /// <summary>
        /// Used to validate some basic assumptions about how type discovery works 
        /// within <see cref="ReflectionDiscovery"/>
        /// </summary>
        [Theory]
        [InlineData(typeof(MemoryMeasurementAttribute), typeof(MemoryMeasurementConfigurator), true)]
        [InlineData(typeof(MemoryMeasurementAttribute), typeof(SimpleMeasurementConfigurator), false)]
        [InlineData(typeof(MemoryAssertionAttribute), typeof(MemoryMeasurementConfigurator), true)]
        [InlineData(typeof(MemoryAssertionAttribute), typeof(SimpleMeasurementConfigurator), false)]
        [InlineData(typeof(SimpleMeasurementAttribute), typeof(SimpleMeasurementConfigurator), true)]
        [InlineData(typeof(DerivedMeasurementAttribute), typeof(DerivedMeasurementConfigurator), true)]
        [InlineData(typeof(DerivedMeasurementAttribute), typeof(SimpleMeasurementConfigurator), true)]
        [InlineData(typeof(SimpleMeasurementAttribute), typeof(DerivedMeasurementConfigurator), false)]
        public void ReflectionDiscoveryCanFindMatchingConfiguratorViaReflection(Type measurementType,
            Type expectedConfiguratorType, bool matchResult)
        {
            Assert.True(ReflectionDiscovery.ConfiguratorSupportsMeasurement(measurementType, expectedConfiguratorType) == matchResult, $"Expected {matchResult} but got ${!matchResult}");
        }



        [Theory]
        [InlineData(typeof(MemoryMeasurementAttribute), typeof(MemoryMeasurementConfigurator))]
        [InlineData(typeof(MemoryAssertionAttribute), typeof(MemoryMeasurementConfigurator))]
        [InlineData(typeof(GcMeasurementAttribute), typeof(GcMeasurementConfigurator))]
        [InlineData(typeof(GcThroughputAssertionAttribute), typeof(GcMeasurementConfigurator))]
        [InlineData(typeof(GcTotalAssertionAttribute), typeof(GcMeasurementConfigurator))]
        [InlineData(typeof(CounterMeasurementAttribute), typeof(CounterMeasurementConfigurator))]
        [InlineData(typeof(CounterThroughputAssertionAttribute), typeof(CounterMeasurementConfigurator))]
        [InlineData(typeof(CounterTotalAssertionAttribute), typeof(CounterMeasurementConfigurator))]
        public void ReflectionDiscoveryShouldFindAllBuiltInConfigurators(Type measurementType,
            Type expectedConfiguratorType)
        {
            var discovery = new ReflectionDiscovery(NoOpBenchmarkOutput.Instance);

            // limit our search to the declaring assembly
            var actualConfiguratorType = discovery.GetConfiguratorTypeForMeasurement(measurementType, measurementType.Assembly);
            Assert.Equal(expectedConfiguratorType, actualConfiguratorType);
        }
        
        [Fact]
        public void ReflectionDiscoveryShouldGetEmptyConfiguratorWhenNoMatchingConfiguratorIsDefined()
        {
            var discovery = new ReflectionDiscovery(NoOpBenchmarkOutput.Instance);
            var unsupportedMeasurement = typeof (UnsupportedMeasurementAttribute);

            var actualConfiguratorType = discovery.GetConfiguratorTypeForMeasurement(unsupportedMeasurement);
            Assert.Equal(MeasurementConfigurator.EmptyConfiguratorType, actualConfiguratorType);
        }
    }
}

