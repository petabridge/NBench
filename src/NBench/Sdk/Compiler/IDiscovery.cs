// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using NBench.Reporting;

namespace NBench.Sdk.Compiler
{
    /// <summary>
    /// Compiler responsible for generating <see cref="BenchmarkSettings"/>
    /// </summary>
    public interface IDiscovery
    {
        /// <summary>
        /// Output engine used to write discovery and processing results to the log
        /// </summary>
        IBenchmarkOutput Output { get; }

        /// <summary>
        /// Engine used to perform BenchmarkAssertions against data collected from a <see cref="Benchmark"/>
        /// </summary>
        IBenchmarkAssertionRunner BenchmarkAssertions { get; }

        /// <summary>
        /// Uses reflection on the target assembly to discover <see cref="PerfBenchmarkAttribute"/>
        /// instances.
        /// </summary>
        /// <param name="targetAssembly">The assembly we're going to scan for benchmarks.</param>
        /// <returns>A list of <see cref="Benchmark"/>s we can run based on the classes found inside <paramref name="targetAssembly"/>.</returns>
        IEnumerable<Benchmark> FindBenchmarks(Assembly targetAssembly);

        /// <summary>
        /// Uses reflection on the target assembly to discover <see cref="PerfBenchmarkAttribute"/>
        /// instances.
        /// </summary>
        /// <param name="targetType">The type we're going to scan for benchmarks.</param>
        /// <returns>A list of <see cref="Benchmark"/>s we can run based on the classes found inside <paramref name="targetType"/>.</returns>
        IEnumerable<Benchmark> FindBenchmarks(Type targetType);
    }
}

