// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Reporting
{
    /// <summary>
    /// <see cref="IBenchmarkOutput"/> implementation that doesn't do anything
    /// </summary>
    public class NoOpBenchmarkOutput : IBenchmarkOutput
    {
        private NoOpBenchmarkOutput() { }

        public static readonly NoOpBenchmarkOutput Instance = new NoOpBenchmarkOutput();

        public void WriteStartingBenchmark(string benchmarkName)
        {
            //no-op
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

