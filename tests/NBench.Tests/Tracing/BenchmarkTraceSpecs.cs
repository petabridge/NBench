using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using Xunit;

namespace NBench.Tests.Tracing
{
    public class BenchmarkTraceSpecs
    {
        public const string CleanupTrace = "Calling trace while shutting down!";
        public const string RunTrace = "My custom trace method!";
        public const string SetupTrace = "Setup trace";
        public const int IterationCount = 10;

        // Pre-warmup + warmups + run
        public const int ExpectedTraces = IterationCount*2 + 1;

        public class TracingBenchmark
        {
           
            [PerfSetup]
            public void Setup(BenchmarkContext context)
            {
                context.Trace.Debug(SetupTrace);
            }

            [PerfBenchmark(TestMode = TestMode.Test, NumberOfIterations = IterationCount, RunTimeMilliseconds = 1000)]
            [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
            [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThan, ByteConstants.EightKb)]
            public void Run1(BenchmarkContext context)
            {
                context.Trace.Debug(RunTrace);
            }

            [PerfCleanup]
            public void Cleanup(BenchmarkContext context)
            {
                context.Trace.Info(CleanupTrace);
            }
        }

        [Fact]
        public void Should_write_trace_messages_in_user_spec_to_output_when_runner_has_enabled_tracing()
        {
            var methodOutput = new List<string>();
            var discovery = new ReflectionDiscovery(new ActionBenchmarkOutput(writeLineAction: str =>
                {
                    methodOutput.Add(str);
                }), DefaultBenchmarkAssertionRunner.Instance, new RunnerSettings() { TracingEnabled = true});

            var benchmarks = discovery.FindBenchmarks(typeof (TracingBenchmark)).ToList();
            Assert.Equal(1, benchmarks.Count); // sanity check
            foreach(var benchmark in benchmarks)
                benchmark.Run();

            var setupTraces = methodOutput.Count(x => x.Contains(SetupTrace));
            var runTraces = methodOutput.Count(x => x.Contains(RunTrace));
            var cleanupTraces = methodOutput.Count(x => x.Contains(CleanupTrace));

            Assert.Equal(ExpectedTraces, setupTraces);
            Assert.Equal(ExpectedTraces, runTraces);
            Assert.Equal(ExpectedTraces, cleanupTraces);
        }

        [Fact]
        public void Should_NOT_write_trace_messages_in_user_spec_to_output_when_runner_has_NOT_enabled_tracing()
        {
            var methodOutput = new List<string>();
            var discovery = new ReflectionDiscovery(new ActionBenchmarkOutput(writeLineAction: str =>
            {
                methodOutput.Add(str);
            }), DefaultBenchmarkAssertionRunner.Instance, new RunnerSettings() { TracingEnabled = false }); //disabled tracing

            var benchmarks = discovery.FindBenchmarks(typeof(TracingBenchmark)).ToList();
            Assert.Equal(1, benchmarks.Count); // sanity check
            foreach (var benchmark in benchmarks)
                benchmark.Run();

            var setupTraces = methodOutput.Count(x => x.Contains(SetupTrace));
            var runTraces = methodOutput.Count(x => x.Contains(RunTrace));
            var cleanupTraces = methodOutput.Count(x => x.Contains(CleanupTrace));

            Assert.Equal(0, setupTraces);
            Assert.Equal(0, runTraces);
            Assert.Equal(0, cleanupTraces);
        }
    }
}
