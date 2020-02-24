// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using NBench.Reporting;
using Xunit.Abstractions;

namespace NBench.Tests
{
    /// <inheritdoc />
    /// <summary>
    /// Used to capturing <see cref="T:NBench.Reporting.IBenchmarkOutput" /> data
    /// inside XUnit specs.
    /// </summary>
    public class XunitBenchmarkOutputHelper : IBenchmarkOutput
    {
        private readonly ITestOutputHelper _helper;

        public XunitBenchmarkOutputHelper(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        public void WriteLine(string message)
        {
            _helper.WriteLine(message);
        }

        public void Warning(string message)
        {
            _helper.WriteLine("WARNING: {0}", message);
        }

        public void Error(Exception ex, string message)
        {
            _helper.WriteLine("ERROR: {0}", message);
            _helper.WriteLine(ex.Message);
            _helper.WriteLine(ex.Source);
            _helper.WriteLine(ex.StackTrace);
        }

        public void Error(string message)
        {
            _helper.WriteLine("ERROR: {0}", message);
        }

        public void StartBenchmark(string benchmarkName)
        {
            _helper.WriteLine("Starting {0}", benchmarkName);
        }

        public void SkipBenchmark(string benchmarkName)
        {
            _helper.WriteLine("Skipping {0}", benchmarkName);
        }

        public void FinishBenchmark(string benchmarkName)
        {
            _helper.WriteLine("Finishing {0}", benchmarkName);
        }

        public void WriteRun(BenchmarkRunReport report, bool isWarmup = false)
        {
            
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            
        }
    }
}

