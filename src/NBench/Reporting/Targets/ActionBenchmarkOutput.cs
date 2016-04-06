// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench.Reporting.Targets
{
    /// <summary>
    /// An <see cref="IBenchmarkOutput"/> designed to run BenchmarkAssertions against the data we collect on each run and in the final benchmark.
    /// </summary>
    public sealed class ActionBenchmarkOutput : IBenchmarkOutput
    {
        private readonly Action<BenchmarkRunReport, bool> _runAction;
        private readonly Action<BenchmarkFinalResults> _benchmarkAction;

        public ActionBenchmarkOutput(Action<BenchmarkRunReport> runAction, Action<BenchmarkFinalResults> benchmarkAction)
            : this((report, b) => runAction(report), benchmarkAction)
        {
        }

        public ActionBenchmarkOutput(Action<BenchmarkRunReport, bool> runAction, Action<BenchmarkFinalResults> benchmarkAction)
        {
            _runAction = runAction;
            _benchmarkAction = benchmarkAction;
        }

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

        public void WriteRun(BenchmarkRunReport report, bool isWarmup = false)
        {
            _runAction(report, isWarmup);
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            _benchmarkAction(results);
        }
    }
}

