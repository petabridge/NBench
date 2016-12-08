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
        private readonly Action<string> _writeAction;

        public static Action<BenchmarkRunReport, bool> DefaultRunAction = (report, b) => { };
        public static Action<BenchmarkFinalResults> DefaultBenchmarkResultsAction = results => { };
        public static Action<string> DefaultWriteLineAction = s => { };

        public ActionBenchmarkOutput(Action<BenchmarkRunReport> runAction, Action<BenchmarkFinalResults> benchmarkAction)
            : this((report, b) => runAction(report), benchmarkAction, DefaultWriteLineAction)
        {
        }

        public ActionBenchmarkOutput(Action<BenchmarkRunReport, bool> runAction = null, Action<BenchmarkFinalResults> benchmarkAction = null, Action<string> writeLineAction = null)
        {
            _runAction = runAction ?? DefaultRunAction;
            _benchmarkAction = benchmarkAction ?? DefaultBenchmarkResultsAction;
            _writeAction = writeLineAction ?? DefaultWriteLineAction;
        }

        public void WriteLine(string message)
        {
            _writeAction(message);
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
            _runAction(report, isWarmup);
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            _benchmarkAction(results);
        }
    }
}

