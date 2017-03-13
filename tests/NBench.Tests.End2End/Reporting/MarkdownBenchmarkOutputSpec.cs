using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBench.Collection;
using NBench.Metrics;
using NBench.Metrics.Counters;
using NBench.Reporting;
using NBench.Reporting.Targets;
#if !CORECLR
using ApprovalTests;
using ApprovalTests.Reporters;
#endif
using NBench.Sdk;
using Xunit;
using Xunit.Abstractions;

namespace NBench.Tests.End2End.Reporting
{
#if !CORECLR
    [UseReporter(typeof(ApprovalTests.Reporters.DiffReporter))]
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
        public void Should_compare_benchmark_output_static_data()
        {
            CleanPerfDir(_perfResultsPath);
            SystemTime.UtcNow = () => new DateTime(2017, 3, 13);

            var mdOutput = new MarkdownBenchmarkOutput(_perfResultsPath);
            var fakeBenchmarkResults = new BenchmarkFinalResults(
                new BenchmarkResults("NBench.FakeBenchmark",
                    new BenchmarkSettings(TestMode.Test, RunMode.Iterations, 30, 1000,
                        new List<IBenchmarkSetting>(),
                        new ConcurrentDictionary<MetricName, MetricsCollectorSelector>()),
                    new List<BenchmarkRunReport>()),
                new List<AssertionResult>());
            mdOutput.WriteBenchmark(fakeBenchmarkResults);
            var fakePerfResultsFile = Directory.GetFiles(_perfResultsPath, "NBench.FakeBenchmark*.md",
                SearchOption.AllDirectories);
            Approvals.Verify(File.ReadAllText(fakePerfResultsFile.FirstOrDefault()));
        }
        private static void CleanPerfDir(string path)
        {
            var files = Directory.GetFiles(path, "*.md", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
#endif

}
