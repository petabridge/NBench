// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NBench.Collection;
using NBench.Metrics;
using NBench.Sdk.Compiler;

namespace NBench.Sdk
{
    /// <summary>
    /// Static utility class for <see cref="IMeasurementConfigurator{T}"/>
    /// </summary>
    public static class MeasurementConfigurator
    {
        /// <summary>
        /// Special case type when we can't find a matching configurator
        /// </summary>
        public class EmptyConfigurator : IMeasurementConfigurator
        {
            private EmptyConfigurator() { }
            public static EmptyConfigurator Instance = new EmptyConfigurator();
            public Type MeasurementType { get; }
            public TypeInfo MeasurementTypeInfo { get; }
            public MetricsCollectorSelector GetMetricsProvider(MeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }

            public IBenchmarkSetting GetBenchmarkSettings(MeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }

            IEnumerable<IBenchmarkSetting> IMeasurementConfigurator.GetBenchmarkSettings(MeasurementAttribute instance)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// <see cref="Type"/> data for <see cref="EmptyConfigurator"/>
        /// </summary>
        public static readonly Type EmptyConfiguratorType = typeof (EmptyConfigurator);

        public static readonly Type ConfiguratorType = typeof (IMeasurementConfigurator);
        /// <summary>
        /// Returns <c>true</c> if the given type implements <see cref="IMeasurementConfigurator"/>
        /// </summary>
        /// <param name="configuratorType">The type we're testing.</param>
        /// <returns>True if it's a valid <see cref="IMeasurementConfigurator"/>, false otherwise.</returns>
        public static bool IsValidConfigurator(Type configuratorType)
        {
            return configuratorType != null
                   && configuratorType != EmptyConfiguratorType
                   && configuratorType.GetTypeInfo().ImplementedInterfaces.Contains(ConfiguratorType);
        }
    }

    /// <summary>
    /// Used by <see cref="ReflectionDiscovery"/> to provide all of the necessary
    /// components needed to create usable settings needed to instrument developer-defined
    /// <see cref="MeasurementAttribute"/>s.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="MeasurementAttribute"/> configured by this class.</typeparam>
    public abstract class MeasurementConfigurator<T> : IMeasurementConfigurator<T> where T:MeasurementAttribute
    {
        private static readonly Type _internalMeasurementType = typeof (T);
        private static readonly TypeInfo _internalMeasurementTypeInfo = _internalMeasurementType.GetTypeInfo();

        /// <summary>
        /// The type of the underlying <see cref="MeasurementAttribute"/> configured by this <see cref="MeasurementConfigurator{T}"/>
        /// </summary>
        public Type MeasurementType => _internalMeasurementType;

        /// <summary>
        /// <see cref="TypeInfo"/> extracted from <see cref="MeasurementType"/>; cached to save on frequent lookups.
        /// </summary>
        public TypeInfo MeasurementTypeInfo => _internalMeasurementTypeInfo;

        public MetricsCollectorSelector GetMetricsProvider(MeasurementAttribute instance)
        {
            var targetAttribute = instance as T;
            if(targetAttribute == null)
                throw new NotSupportedException($"{instance.GetType()} is not supported by {GetType()}");
            return GetMetricsProvider(targetAttribute);
        }

        public IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(MeasurementAttribute instance)
        {
            var targetAttribute = instance as T;
            if (targetAttribute == null)
                throw new NotSupportedException($"{instance.GetType()} is not supported by {GetType()}");
            return GetBenchmarkSettings(targetAttribute);
        }

        /// <summary>
        /// Produce a <see cref="MetricName"/> implementation based on the provided <see cref="MeasurementAttribute"/> instance.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>A <see cref="MetricName"/> implementation corresponding to this metric.</returns>
        public abstract MetricName GetName(T instance);

        /// <summary>
        /// Produce a <see cref="MetricsCollectorSelector"/> implementation based on the provided <see cref="MeasurementAttribute"/> instance.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>A <see cref="MetricsCollectorSelector"/> implementation corresponding to this metric.</returns>
        /// <remarks>This object basically tells NBench how to gather the metrics associated with the <see cref="MeasurementAttribute"/></remarks>
        public abstract MetricsCollectorSelector GetMetricsProvider(T instance);

        /// <summary>
        /// Produce a <see cref="IBenchmarkSetting"/> implementation that will be used to tell the
        /// <see cref="Benchmark"/> class which <see cref="Assertion"/>, if any, it should perform against the
        /// <see cref="MetricCollector"/> data produced for this setting by the <see cref="GetMetricsProvider(NBench.MeasurementAttribute)"/> method
        /// on this configurator.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns><see cref="IBenchmarkSetting"/> implementation instances built specifically for T</returns>
        public abstract IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(T instance);
    }
}

