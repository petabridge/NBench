using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NBench.Collection;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Metrics.GarbageCollection;
using NBench.Metrics.Memory;
using NBench.Reporting;
using NBench.Reporting.Targets;
#if !CORECLR
using ApprovalTests;
using ApprovalTests.Reporters;
using ApprovalTests.Scrubber;
#endif
using NBench.Sdk;
using NBench.Sys;
using Xunit;
using Xunit.Abstractions;

namespace NBench.Tests.End2End.Reporting
{
#if !CORECLR
    [UseReporter(typeof(DiffReporter))]
    public class MarkdownBenchmarkOutputSpec
    {
        private readonly ITestOutputHelper _output;
        private static readonly string _solutionPath =
            ".." + Path.DirectorySeparatorChar
            + ".." + Path.DirectorySeparatorChar
            + ".." + Path.DirectorySeparatorChar
            + ".." + Path.DirectorySeparatorChar
            + ".." + Path.DirectorySeparatorChar;
        private static readonly string _perfResultsPath =
            Path.GetFullPath(_solutionPath + "PerfResults");

        public MarkdownBenchmarkOutputSpec(ITestOutputHelper ouput)
        {
            _output = ouput;
        }

        [Fact]
        public void Should_report_valid_markdown_format()
        {
            CleanPerfDir(_perfResultsPath);
            SystemTime.UtcNow = () => new DateTime(2017, 3, 13);

            var mdOutput = new MarkdownBenchmarkOutput(_perfResultsPath);
            var fakeBenchmarkResults = new BenchmarkResults("NBench.FakeBenchmark",
                new BenchmarkSettings(TestMode.Test, RunMode.Iterations, 30, 1000,
                    new List<IBenchmarkSetting>(),
                    new ConcurrentDictionary<MetricName, MetricsCollectorSelector>()),
                new List<BenchmarkRunReport>()
                {
                    new BenchmarkRunReport(TimeSpan.FromSeconds(3),
                        new List<MetricRunReport>()
                        {
                            new MetricRunReport(new CounterMetricName("FakeCounterMetric"), "bytes", 0d, Stopwatch.Frequency),
                            new MetricRunReport(new GcMetricName(GcMetric.TotalCollections, GcGeneration.Gen2), "collections", 0d, Stopwatch.Frequency),
                            new MetricRunReport(new MemoryMetricName(MemoryMetric.TotalBytesAllocated), "operations", 0d, Stopwatch.Frequency),
                        },
                        new List<Exception>())
                });
            var fakeBenchmarkFinalResults = new BenchmarkFinalResults(fakeBenchmarkResults, new List<AssertionResult>()
            {
                new AssertionResult(new CounterMetricName("FakeCounterMetric"), "[Counter] FakeCounterMetric Assertion Result", true),
                new AssertionResult(new GcMetricName(GcMetric.TotalCollections, GcGeneration.Gen2), "TotalCollections [Gen2] Assertion Result", true),
                new AssertionResult(new MemoryMetricName(MemoryMetric.TotalBytesAllocated), "TotalBytesAllocated Assertion Result", true)
            });
            mdOutput.WriteBenchmark(fakeBenchmarkFinalResults);

            var fakePerfResultsFile = Directory.GetFiles(_perfResultsPath, "NBench.FakeBenchmark*",
                SearchOption.AllDirectories);

            Approvals.Verify(File.ReadAllText(fakePerfResultsFile.FirstOrDefault()), ScrubSysInfo);
        }

        private static void CleanPerfDir(string path)
        {
            var files = Directory.GetFiles(path, "NBench.FakeBenchmark*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        private static string ScrubSysInfo(string input)
        {
            var startIndex = input.IndexOf(@"```ini");
            var endIndex = input.IndexOf(@"```" + Environment.NewLine, startIndex) + 3;
            return input.Remove(startIndex, endIndex - startIndex);
        }
    }
#endif
}
