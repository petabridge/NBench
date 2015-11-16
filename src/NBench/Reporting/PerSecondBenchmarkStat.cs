// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace NBench.Reporting
{
    /// <summary>
    /// Computes <see cref="BenchmarkStat"/> relative to the amount of time it took to collect those
    /// </summary>
    public struct PerSecondBenchmarkStat
    {
        public PerSecondBenchmarkStat(BenchmarkStat stat, double totalElapsedSeconds)
        {
            totalElapsedSeconds = totalElapsedSeconds.Equals(0.0) ? 1.0 : totalElapsedSeconds;
            Min = stat.Min/totalElapsedSeconds;
            Max = stat.Max/totalElapsedSeconds;
            Average = stat.Average/totalElapsedSeconds;
            Sum = stat.Sum/totalElapsedSeconds;
            StandardDeviation = stat.StandardDeviation/totalElapsedSeconds;
            StandardError = stat.StandardError/totalElapsedSeconds;
        }

        public double Min { get; }

        public double Max { get; }

        public double Average { get; }

        public double Sum { get; }

        public double StandardDeviation { get; }

        public double StandardError { get; }
    }
}

