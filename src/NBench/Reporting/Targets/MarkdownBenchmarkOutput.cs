using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NBench.Sys;
using NBench.Util;

namespace NBench.Reporting.Targets
{
    /// <summary>
    /// <see cref="IBenchmarkOutput"/> implementation that writes output for each
    /// completed benchmark to a markdown file. Uses <see cref="FileNameGenerator"/>
    /// to generate a file name unique to each test AND the time it was run.
    /// </summary>
    public class MarkdownBenchmarkOutput : IBenchmarkOutput
    {
        public const string MarkdownFileExtension = ".md";
        public const int MaxColumnSize = 18;
        public const string MarkdownTableColumnEnd = " |";

        public void WriteLine(string message)
        {
            // no-op
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

        public void WriteRun(BenchmarkRunReport report, bool isWarmup = false)
        {
            // no-op
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            var filePath = FileNameGenerator.GenerateFileName(results.BenchmarkName, MarkdownFileExtension);
            var sysInfo = SysInfo.Instance;
            var sb = new StringBuilder();
            sb.AppendLine($"# {results.BenchmarkName}");
            sb.AppendLine($"_{DateTime.UtcNow}_");
            sb.AppendLine("### System Info");
            sb.AppendLine("```ini");
            sb.AppendLine($"NBench={sysInfo.NBenchAssemblyVersion}");
            sb.AppendLine($"OS={sysInfo.OS}");
            sb.AppendLine($"ProcessorCount={sysInfo.ProcessorCount}");
            sb.AppendLine(
                $"CLR={sysInfo.ClrVersion},IsMono={sysInfo.IsMono},MaxGcGeneration={sysInfo.MaxGcGeneration}");
            sb.AppendLine($"WorkerThreads={sysInfo.WorkerThreads}, IOThreads={sysInfo.IOThreads}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("### NBench Settings");
            sb.AppendLine("```ini");
            sb.AppendLine($"RunMode={results.Data.Settings.RunMode}, TestMode={results.Data.Settings.TestMode}");
            sb.AppendLine($"NumberOfIterations={results.Data.Settings.NumberOfIterations}, MaximumRunTime={results.Data.Settings.RunTime}");
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
            if (results.AssertionResults.Count > 0)
            {
                sb.AppendLine("## Assertions");
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
            var report = sb.ToString();

            File.WriteAllText(filePath, report, Encoding.UTF8);
        }

        public static string BuildStatTable(IEnumerable<AggregateMetrics> metrics, int columnWidth = MaxColumnSize)
        {
            var sb = new StringBuilder();
            sb.Append(("Metric"+MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Units" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Max" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Average" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Min" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("StdDev" + MarkdownTableColumnEnd).PadLeft(columnWidth));
            sb.AppendLine();
            sb.Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'));
            sb.AppendLine();
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

        public static string BuildPerSecondsStatTable(IEnumerable<AggregateMetrics> metrics, int columnWidth = MaxColumnSize)
        {
            var sb = new StringBuilder();
            sb.Append(("Metric" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Units / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Max / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Average / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("Min / s" + MarkdownTableColumnEnd).PadLeft(columnWidth))
                .Append(("StdDev / s" + MarkdownTableColumnEnd).PadLeft(columnWidth));
            sb.AppendLine();
            sb.Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'))
                .Append(MarkdownTableColumnEnd.PadLeft(columnWidth, '-'));
            sb.AppendLine();
            foreach (var metric in metrics)
            {
                sb.Append((metric.Name + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.Unit + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.PerSecondStats.Max.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.PerSecondStats.Average.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.PerSecondStats.Min.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.Append((metric.PerSecondStats.StandardDeviation.ToString("N") + MarkdownTableColumnEnd).PadLeft(columnWidth));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
