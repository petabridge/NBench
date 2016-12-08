// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace NBench.Reporting
{
    /// <summary>
    /// Composition of multiple <see cref="IBenchmarkOutput"/> instances being run in parallel.
    /// </summary>
    public class CompositeBenchmarkOutput : IBenchmarkOutput
    {
        private readonly IReadOnlyList<IBenchmarkOutput> _outputs;

        public CompositeBenchmarkOutput(params IBenchmarkOutput[] outputs) : this(new List<IBenchmarkOutput>(outputs))
        {
        }

        public CompositeBenchmarkOutput(IReadOnlyList<IBenchmarkOutput> outputs)
        {
            Contract.Requires(outputs != null);
            _outputs = outputs;
        }

        public void WriteLine(string message)
        {
            foreach(var o in _outputs)
                o.WriteLine(message);
        }

        public void Warning(string message)
        {
            foreach(var o in _outputs)
                o.Warning(message);
        }

        public void Error(Exception ex, string message)
        {
            foreach (var o in _outputs)
                o.Error(ex, message);
        }

        public void Error(string message)
        {
            foreach (var o in _outputs)
                o.Error(message);
        }

        public void StartBenchmark(string benchmarkName)
        {
            foreach(var o in  _outputs)
                o.StartBenchmark(benchmarkName);
        }

        public void SkipBenchmark(string benchmarkName)
        {
            foreach(var o in _outputs)
                o.SkipBenchmark(benchmarkName);
        }

        public void FinishBenchmark(string benchmarkName)
        {
            foreach(var o in _outputs)
                o.FinishBenchmark(benchmarkName);
        }

        public void WriteRun(BenchmarkRunReport report, bool isWarmup = false)
        {
            foreach(var o in _outputs)
                o.WriteRun(report, isWarmup);
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            foreach(var o in _outputs)
                o.WriteBenchmark(results);
        }
    }
}

