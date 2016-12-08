// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench.Reporting
{
    /// <summary>
    /// <see cref="IBenchmarkOutput"/> implementation that doesn't do anything
    /// </summary>
    public class NoOpBenchmarkOutput : IBenchmarkOutput
    {
        private NoOpBenchmarkOutput() { }

        public static readonly NoOpBenchmarkOutput Instance = new NoOpBenchmarkOutput();

        public void WriteLine(string message)
        {
            //no-op
        }

        public void Warning(string message)
        {
            //no-op
        }

        public void Error(Exception ex, string message)
        {
            //no-op
        }

        public void Error(string message)
        {
            //no-op
        }

        public void StartBenchmark(string benchmarkName)
        {
            // no-op
        }

        public void SkipBenchmark(string benchmarkName)
        {
            // no-op
        }

        public void FinishBenchmark(string benchmarkName)
        {
            // no-op
        }

        public void WriteRun(BenchmarkRunReport report, bool isWarmup = false)
        {
            //no-op
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            //no-op
        }
    }
}

