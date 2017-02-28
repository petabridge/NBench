// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NBench.Metrics;
using NBench.Sys;
using NBench.Tracing;
using NBench.Util;

namespace NBench.Reporting.Targets
{
    /// <summary>
    ///     <see cref="IBenchmarkOutput" /> implementation that writes output for each
    ///     completed benchmark to a markdown file. Uses <see cref="FileNameGenerator" />
    ///     to generate a file name unique to each test AND the time it was run.
    /// </summary>
    public class MarkdownBenchmarkOutput : IBenchmarkOutput
    {
        public const string MarkdownFileExtension = ".md";
        public const int MaxColumnSize = 18;
        public const string MarkdownTableColumnEnd = " |";
        private readonly string _outputDirectory;
        private readonly Lazy<StringBuilder> _traceStringBuilder = new Lazy<StringBuilder>(() => new StringBuilder(), true);

        public MarkdownBenchmarkOutput(string outputDirectory)
        {
            _outputDirectory = outputDirectory;
        }

        public void WriteLine(string message)
        {
            if(message.StartsWith(TraceMessage.TraceIndicator))
                _traceStringBuilder.Value.AppendLine(message);
        }

        public void Warning(string message)
        {
            // no-op
        }

        public void Error(Exception ex, string message)
        {
            // no-op
        }

        public void Error(string message)
        {
            // no-op
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
            // no-op
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            var filePath = FileNameGenerator.GenerateFileName(_outputDirectory, results.BenchmarkName,
                MarkdownFileExtension, DateTime.UtcNow);
            var sysInfo = SysInfo.Instance;
            var sb = new StringBuilder();
            sb.AppendLine($"# {results.BenchmarkName}");
            if (!string.IsNullOrEmpty(results.Data.Settings.Description))
            {
                sb.AppendLine($"__{results.Data.Settings.Description}__");
            }
            sb.AppendLine($"_{DateTime.UtcNow}_");
            sb.AppendLine("### System Info");
            sb.AppendLine("```ini");
            sb.AppendLine($"NBench={sysInfo.NBenchAssemblyVersion}");
            sb.AppendLine($"OS={sysInfo.OS}");
            sb.AppendLine($"ProcessorCount={sysInfo.ProcessorCount}");
            sb.AppendLine(
                $"CLR={sysInfo.ClrVersion},IsMono={sysInfo.IsMono},MaxGcGeneration={sysInfo.MaxGcGeneration}");
#if THREAD_POOL
            sb.AppendLine($"WorkerThreads={sysInfo.WorkerThreads}, IOThreads={sysInfo.IOThreads}");
#endif
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("### NBench Settings");
            sb.AppendLine("```ini");
            sb.AppendLine($"RunMode={results.Data.Settings.RunMode}, TestMode={results.Data.Settings.TestMode}");
            if (results.Data.Settings.SkipWarmups)
            {
                sb.AppendLine($"SkipWarmups={results.Data.Settings.SkipWarmups}");
            }
            sb.AppendLine(
                $"NumberOfIterations={results.Data.Settings.NumberOfIterations}, MaximumRunTime={results.Data.Settings.RunTime}");
            sb.AppendLine($"Concurrent={results.Data.Settings.ConcurrentMode}");
            sb.AppendLine($"Tracing={results.Data.Settings.TracingEnabled}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## Data");
            sb.AppendLine("-------------------");
            sb.AppendLine();
            sb.AppendLine("### Totals");
            sb.AppendFormat(BuildStatTable(results.Data.StatsByMetric.Values));
            sb.AppendLine();
            sb.AppendLine("### Per-second Totals");
            sb.AppendFormat(BuildPerSecondsStatTable(results.Data.StatsByMetric.Values));
            sb.AppendLine();
            sb.AppendLine("### Raw Data");
            sb.AppendFormat(BuildRunTable(results.Data.Runs));
            if (results.AssertionResults.Count > 0)
            {
                sb.AppendLine("## Benchmark Assertions");
                sb.AppendLine();
                foreach (var assertion in results.AssertionResults)
                {
                    sb.AppendLine("* " + assertion.Message);
                }
                sb.AppendLine();
            }

            if (results.Data.IsFaulted)
            {
                Console.WriteLine("## Exceptions");
                foreach (var exception in results.Data.Exceptions)
                {
                    sb.AppendLine("```");
                    sb.AppendLine(exception.ToString());
                    sb.AppendLine("```");
                    sb.AppendLine();
                }
            }

            if (_traceStringBuilder.IsValueCreated) // append trace values if the tracing system was used
            {
                sb.AppendLine("## Traces");
                sb.AppendLine(_traceStringBuilder.Value.ToString());
                sb.AppendLine();

                // reset the string builder, since it may be re-used on the next benchmark
                _traceStringBuilder.Value.Clear();
            }

            var report = sb.ToString();

			// ensure directory exists
			var directoryPath = Path.GetDirectoryName(filePath);

			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);

            File.WriteAllText(filePath, report, Encoding.UTF8);
        }

        private static string BuildStatTable(IEnumerable<AggregateMetrics> metrics, int columnWidth = MaxColumnSize)
        {
            var sb = new StringBuilder();
            sb.Append(("Metric" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Units" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Max" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Average" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Min" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("StdDev" + MarkdownTableColumnEnd).PadLeft(columnWidth));
            sb.AppendLine();
            AddMarkdownTableHeaderRow(sb, columnWidth);
            foreach (var metric in metrics)
            {
                sb.Append((metric.Name + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.Unit + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.Stats.Max.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.Stats.Average.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.Stats.Min.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.Stats.StandardDeviation.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string BuildPerSecondsStatTable(IEnumerable<AggregateMetrics> metrics,
            int columnWidth = MaxColumnSize)
        {
            var sb = new StringBuilder();
            sb.Append(("Metric" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Units / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Max / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Average / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Min / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("StdDev / s" + MarkdownTableColumnEnd).PadLeft(columnWidth));
            sb.AppendLine();
            AddMarkdownTableHeaderRow(sb, columnWidth);
            foreach (var metric in metrics)
            {
                sb.Append((metric.Name + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.Unit + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.PerSecondStats.Max.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.PerSecondStats.Average.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.PerSecondStats.Min.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append(
                    (metric.PerSecondStats.StandardDeviation.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string BuildRunTable(IReadOnlyList<BenchmarkRunReport> runs, int columnWidth = MaxColumnSize)
        {
            var sb = new StringBuilder();

            var groupByMetrics = new Dictionary<MetricName, List<MetricRunReport>>();
            var runNumber = 0;

            /*
             * Organize runs by metric
             */
            foreach (var run in runs)
            {
                foreach (var metric in run.Metrics)
                {
                    if (!groupByMetrics.ContainsKey(metric.Key))
                    {
                        groupByMetrics[metric.Key] = new List<MetricRunReport>(runs.Count);
                    }
                    groupByMetrics[metric.Key].Add(metric.Value);
                }
                ++runNumber;
            }

            /*
             * Print runs by metric
             */
            foreach (var metric in groupByMetrics)
            {
                sb.AppendLine($"#### {metric.Key}");
                if (metric.Value.Count == 0)
                {
                    sb.AppendLine("_No values recorded._");
                    continue;
                }

                var unitOfMeasure = metric.Value.First().Unit;

                sb.Append(("Run #" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                    .Append((unitOfMeasure + MarkdownTableColumnEnd).PadLeft(columnWidth))
                    .Append(($"{unitOfMeasure} / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                    .Append(($"ns / {unitOfMeasure}" + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.AppendLine();
                AddMarkdownTableHeaderRow(sb, columnWidth, 4);
                var i = 1;
                foreach (var record in metric.Value)
                {
                    sb.Append((i + MarkdownTableColumnEnd).PadLeft(columnWidth))
                        .Append(
                            ($"{record.MetricValue:n}" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                        .Append(
                            ($"{record.MetricValuePerSecond:n}" + MarkdownTableColumnEnd).PadLeft(
                                columnWidth))
                        .Append(
                            ($"{record.NanosPerMetricValue:n}" + MarkdownTableColumnEnd).PadLeft(
                                columnWidth));
                    sb.AppendLine();
                    i++;
                }
                sb.AppendLine();
            }

            sb.AppendLine();

            return sb.ToString();
        }

        private static void AddMarkdownTableHeaderRow(StringBuilder sb, int columnWidth = MaxColumnSize, int columns = 6)
        {
            for (var i = 0; i < columns; i++)
            {
                sb.Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'));
            }
            sb.AppendLine();
        }
    }
}