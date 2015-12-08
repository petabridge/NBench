using System;
using System.Collections.Generic;
using System.Linq;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;

namespace NBench.Tests.Performance
{
    public class BenchmarkMemoryAllocationSpec
    {
        private static readonly IBenchmarkOutput _benchmarkOutput = new ActionBenchmarkOutput(report => { }, results =>
        {
            //no-op
        });

        private Benchmark TestableBenchmark;
        
        [PerfSetup]
        public void Setup()
        {
            var benchmarkData = ReflectionDiscovery.CreateBenchmarksForClass(typeof (MemoryAllocationSpec)).First();
            var settings = ReflectionDiscovery.CreateSettingsForBenchmark(benchmarkData);
            var invoker = ReflectionDiscovery.CreateInvokerForBenchmark(benchmarkData);
            TestableBenchmark = new Benchmark(settings, invoker, _benchmarkOutput);
        }

        [PerfBenchmark(Description = "Do benchmark instances cleanup after themselves?", NumberOfIterations = 13, RunMode = RunMode.Iterations, TestMode = TestMode.Measurement)]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        [GcMeasurement(GcMetric.TotalCollections, GcGeneration.AllGc)]
        public void BenchmarkRunAllocation()
        {
            TestableBenchmark.Run();
        }
    }

    public class MemoryAllocationSpec
    {
        private const int NumOfByteArrays = 10000;
        private const int ByteArraySize = 1024; // 1kb
        private const long FudgeFactor = ByteConstants.SixteenKb;
        private IList<byte[]> byteArrays = new List<byte[]>();

        [PerfBenchmark(Description = "Simple test that allocates many byte arrays into a list", 
            RunMode = RunMode.Iterations, NumberOfIterations = 13, 
            TestMode = TestMode.Measurement)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, NumOfByteArrays * ByteArraySize + FudgeFactor)]
        public void ByteArrayAllocation()
        {
            for (var i = 0; i < NumOfByteArrays; --i)
            {
                byteArrays.Add(new byte[ByteArraySize]);
            }
        }
    }
}
