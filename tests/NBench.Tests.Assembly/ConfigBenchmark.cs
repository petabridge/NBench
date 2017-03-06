// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if  CORECLR
using Microsoft.Extensions.Configuration;
#else
using System.Configuration;
#endif

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

#if CORECLR
            var builder = new ConfigurationBuilder();
            builder.AddXmlFile("App.config");

            var config = builder.Build();
            if (config["TestKey"] != "42")
                throw new InvalidOperationException("TestKey from AppSettings could not be loaded!");
#else
            if (ConfigurationManager.AppSettings["TestKey"] != "42")
                throw new InvalidOperationException("TestKey from AppSettings could not be loaded!");
#endif
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

