// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Metrics;
using NBench.Sdk;

namespace NBench.Reporting
{
    /// <summary>
    ///     Interface used to write <see cref="BenchmarkRunReport" /> and <see cref="BenchmarkResults" />
    ///     out to various reporting mechansims, including file-based and console-based ones.
    /// </summary>
    public interface IBenchmarkOutput
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="benchmarkName"></param>
        void WriteStartingBenchmark(string benchmarkName);

        /// <summary>
        ///     Write out an individual run to the console or file
        /// </summary>
        /// <param name="report">The report for an individual <see cref="BenchmarkRun" /></param>
        /// <param name="isWarmup">If <c>true</c>, the output writer will signal that this data is for a warm-up run.</param>
        void WriteRun(BenchmarkRunReport report, bool isWarmup = false);

        /// <summary>
        ///     Write out the entire benchmark result set to the console or file
        /// </summary>
        /// <param name="results">
        ///     The report for all <see cref="BenchmarkRun" />s in a <see cref="Benchmark" />, including
        ///     <see cref="Assertion" /> results.
        /// </param>
        void WriteBenchmark(BenchmarkFinalResults results);
    }
}

