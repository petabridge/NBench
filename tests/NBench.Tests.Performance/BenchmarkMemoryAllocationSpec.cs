// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Linq;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;

namespace NBench.Tests.Performance
{
    public class BenchmarkMemoryAllocationSpec
    {
        private static readonly IBenchmarkOutput BenchmarkOutput = new ActionBenchmarkOutput(report => { }, results =>
        {
            //no-op
        });

        private Benchmark _testableBenchmark;

        [PerfSetup]
        public void Setup()
        {
            var benchmarkData = ReflectionDiscovery.CreateBenchmarksForClass(typeof (MemoryAllocationSpec)).First();
            var settings = ReflectionDiscovery.CreateSettingsForBenchmark(benchmarkData);
            var invoker = ReflectionDiscovery.CreateInvokerForBenchmark(benchmarkData);
            _testableBenchmark = new Benchmark(settings, invoker, BenchmarkOutput);
        }

        [PerfBenchmark(Description = "Do benchmark instances cleanup after themselves?", NumberOfIterations = 13,
            RunMode = RunMode.Iterations, TestMode = TestMode.Measurement, Skip = "Easy on the memory allocation")]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void BenchmarkRunAllocation()
        {
            _testableBenchmark.Run();
        }
    }
}

