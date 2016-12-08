// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using NBench.Collection;
using NBench.Metrics;
using NBench.Sdk.Compiler;

namespace NBench.Sdk
{
    /// <summary>
    /// Interface used to configure <see cref="MeasurementAttribute"/>s implemented in both built-in NBench libraries
    /// and external libraries.
    /// </summary>
    /// <typeparam name="T">The type of measurement supported by this configurator</typeparam>
    public interface IMeasurementConfigurator<in T> : IMeasurementConfigurator where T : MeasurementAttribute
    {
        /// <summary>
        /// Produce a <see cref="MetricName"/> implementation based on the provided <see cref="MeasurementAttribute"/> instance.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>A <see cref="MetricName"/> implementation corresponding to this metric.</returns>
        MetricName GetName(T instance);

        /// <summary>
        /// Produce a <see cref="MetricsCollectorSelector"/> implementation based on the provided <see cref="MeasurementAttribute"/> instance.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>A <see cref="MetricsCollectorSelector"/> implementation corresponding to this metric.</returns>
        /// <remarks>This object basically tells NBench how to gather the metrics associated with the <see cref="MeasurementAttribute"/></remarks>
        MetricsCollectorSelector GetMetricsProvider(T instance);

        /// <summary>
        /// Produce a <see cref="IBenchmarkSetting"/> implementation that will be used to tell the
        /// <see cref="Benchmark"/> class which <see cref="Assertion"/>, if any, it should perform against the
        /// <see cref="MetricCollector"/> data produced for this setting by the <see cref="MeasurementConfigurator{T}.GetMetricsProvider(NBench.MeasurementAttribute)"/> method
        /// on this configurator.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>An <see cref="IBenchmarkSetting"/> implementation instance built specifically for <typeparamref name="T"/></returns>
        IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(T instance);
    }

    /// <summary>
    /// Non-generic implementation of <see cref="IMeasurementConfigurator"/>.
    /// 
    /// INTERNAL USE ONLY. Must implement <see cref="IMeasurementConfigurator{T}"/> for NBench <see cref="ReflectionDiscovery"/>
    /// to pick up your metrics.
    /// </summary>
    public interface IMeasurementConfigurator
    {
        /// <summary>
        /// The type of the underlying <see cref="MeasurementAttribute"/> configured by this <see cref="MeasurementConfigurator{T}"/>
        /// </summary>
        Type MeasurementType { get; }

        /// <summary>
        /// <see cref="TypeInfo"/> extracted from <see cref="MeasurementType"/>; cached to save on frequent lookups.
        /// </summary>
        TypeInfo MeasurementTypeInfo { get; }

        /// <summary>
        /// Produce a <see cref="MetricsCollectorSelector"/> implementation based on the provided <see cref="MeasurementAttribute"/> instance.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>A <see cref="MetricsCollectorSelector"/> implementation corresponding to this metric.</returns>
        /// <remarks>This object basically tells NBench how to gather the metrics associated with the <see cref="MeasurementAttribute"/></remarks>
        MetricsCollectorSelector GetMetricsProvider(MeasurementAttribute instance);

        /// <summary>
        /// Produce a <see cref="IBenchmarkSetting"/> implementation that will be used to tell the
        /// <see cref="Benchmark"/> class which <see cref="Assertion"/>, if any, it should perform against the
        /// <see cref="MetricCollector"/> data produced for this setting by the <see cref="MeasurementConfigurator{T}.GetMetricsProvider(NBench.MeasurementAttribute)"/> method
        /// on this configurator.
        /// </summary>
        /// <param name="instance">
        /// An instance of the <see cref="MeasurementAttribute"/> type that corresponds to this configurator. 
        /// Must not be <c>null</c>.
        /// </param>
        /// <returns>A list of <see cref="IBenchmarkSetting"/> implementation instances built specifically for <paramref name="instance"/></returns>
        IEnumerable<IBenchmarkSetting> GetBenchmarkSettings(MeasurementAttribute instance);
    }
}

