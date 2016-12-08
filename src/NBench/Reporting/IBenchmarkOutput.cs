// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
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
        /// Write a line to the NBench output
        /// </summary>
        /// <param name="message">The message we're going to write to output.</param>
        void WriteLine(string message);

        /// <summary>
        /// Write a warning to the NBench output
        /// </summary>
        /// <param name="message">The message we're going to write to output.</param>
        void Warning(string message);

        /// <summary>
        /// Write an error to the NBench output
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> raised during the benchmark.</param>
        /// <param name="message">The message we're going to write to output.</param>
        void Error(Exception ex, string message);

        /// <summary>
        /// Write an error to the NBench output
        /// </summary>
        /// <param name="message">The message we're going to write to output.</param>
        void Error(string message);

        /// <summary>
        /// Signal that we're going to begin a new benchmark
        /// </summary>
        /// <param name="benchmarkName">The name of the benchmark.</param>
        void StartBenchmark(string benchmarkName);

        /// <summary>
        /// Signal that we're going to be skipping a benchmark
        /// </summary>
        /// <param name="benchmarkName">The name of the benchmark.</param>
        void SkipBenchmark(string benchmarkName);

        /// <summary>
        /// Signals that we've completed processing a benchmark, regardless
        /// of how it finished.
        /// </summary>
        /// <param name="benchmarkName">The name of the benchmark.</param>
        void FinishBenchmark(string benchmarkName);

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

