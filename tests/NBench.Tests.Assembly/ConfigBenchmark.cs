﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBench.Tests.Assembly
{
    /// <summary>
    /// A benchmark requiring values from config file
    /// </summary>
    public class ConfigBenchmark
    {
        public const string CounterName = "DumbCounter";
        private Counter _counter;

        [PerfSetup]
        public void SetUp(BenchmarkContext context)
        {
            _counter = context.GetCounter(CounterName);

            if (ConfigurationManager.AppSettings["TestKey"] != "42")
                throw new InvalidOperationException("TestKey from AppSettings could not be loaded!");
        }

        /// <summary>
        /// Run 3 tests, 1 second long each
        /// </summary>
        [PerfBenchmark(Description = "Simple iteration collection test", RunMode = RunMode.Iterations, TestMode = TestMode.Test, RunTimeMilliseconds = 1000, NumberOfIterations = 30)]
        [CounterMeasurement(CounterName)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
        [GcTotalAssertion(GcMetric.TotalCollections, GcGeneration.Gen2, MustBe.ExactlyEqualTo, 0d)]
        public void Run(BenchmarkContext context)
        {
            _counter.Increment();
        }
    }
}
