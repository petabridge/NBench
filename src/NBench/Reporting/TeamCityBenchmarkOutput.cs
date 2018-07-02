// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NBench.Reporting
{
    /// <summary>
    /// TeamCity output formatter.
    /// 
    /// Complies with https://confluence.jetbrains.com/display/TCD10/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingTests
    /// to ensure that output reports from NBench render nicely on TeamCity.
    /// </summary>
    /// <remarks>
    /// Can be enabled in the default NBench test runner by passing in the <code>teamcity=true</code> flag.
    /// </remarks>
    public sealed class TeamCityBenchmarkOutput : IBenchmarkOutput
    {
        private readonly TextWriter _outputWriter;

        /// <summary>
        /// Constructor that takes a <see cref="TextWriter"/> to use
        /// as the output target.
        /// </summary>
        /// <param name="writer">Output target.</param>
        public TeamCityBenchmarkOutput(TextWriter writer)
        {
            _outputWriter = writer;
        }

        /// <summary>
        /// Default constructor. Uses <see cref="Console.Out"/> as the output target.
        /// </summary>
        public TeamCityBenchmarkOutput() : this(Console.Out)
        {
            
        }

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

        public void StartBenchmark(string benchmarkName)
        {
            _outputWriter.WriteLine($"##teamcity[testStarted name=\'{Escape(benchmarkName)}\']");
        }

        public void SkipBenchmark(string benchmarkName)
        {
            _outputWriter.WriteLine($"##teamcity[testIgnored name=\'{Escape(benchmarkName)}\' message=\'skipped\']");
        }

        public void FinishBenchmark(string benchmarkName)
        {
            _outputWriter.WriteLine($"##teamcity[testFinished name=\'{Escape(benchmarkName)}\']");
        }

        public void WriteRun(BenchmarkRunReport report, bool isWarmup = false)
        {
            // no-op
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            BenchmarkStdOut(results, "DATA");
            foreach (var metric in results.Data.StatsByMetric.Values)
            {
                BenchmarkStdOut(results, string.Format("{0}: Max: {2:n} {1}, Average: {3:n} {1}, Min: {4:n} {1}, StdDev: {5:n} {1}", metric.Name,
                    metric.Unit, metric.Stats.Max, metric.Stats.Average, metric.Stats.Min, metric.Stats.StandardDeviation));

                BenchmarkStdOut(results, string.Format("{0}: Max / s: {2:n} {1}, Average / s: {3:n} {1}, Min / s: {4:n} {1}, StdDev / s: {5:n} {1}", metric.Name,
                    metric.Unit, metric.PerSecondStats.Max, metric.PerSecondStats.Average, metric.PerSecondStats.Min, metric.PerSecondStats.StandardDeviation));
            }

            if (results.AssertionResults.Count > 0)
            {
                BenchmarkStdOut(results, "ASSERTIONS");
                foreach (var assertion in results.AssertionResults)
                {
                    if(assertion.Passed)
                        BenchmarkStdOut(results, assertion.Message);
                    else
                    {
                        BenchmarkStdErr(results, assertion.Message);
                    }
                }
            }

            if (results.Data.IsFaulted)
            {
                BenchmarkStdErr(results, "EXCEPTIONS");
                foreach (var exception in results.Data.Exceptions)
                {
                   BenchmarkStdErr(results, exception.ToString());
                }
            }

            if (results.Data.IsFaulted || results.AssertionResults.Any(x => !x.Passed))
            {
                _outputWriter.WriteLine($"##teamcity[testFailed name=\'{Escape(results.BenchmarkName)}\' message=\'Failed at least one assertion or threw exception.\']");
            }
        }

        private void BenchmarkStdOut(BenchmarkFinalResults results, string str)
        {
            _outputWriter.WriteLine($"##teamcity[testStdOut name=\'{Escape(results.BenchmarkName)}\' out=\'{Escape(str)}\']");
        }

        private void BenchmarkStdErr(BenchmarkFinalResults results, string str)
        {
            _outputWriter.WriteLine($"##teamcity[testStdErr name=\'{Escape(results.BenchmarkName)}\' out=\'{Escape(str)}\']");
        }

        private static string Escape(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("|", "||")
                .Replace("'", "|'")
                .Replace("\n", "|n")
                .Replace("\r", "|r")
                .Replace(char.ConvertFromUtf32(int.Parse("0086", NumberStyles.HexNumber)), "|x")
                .Replace(char.ConvertFromUtf32(int.Parse("2028", NumberStyles.HexNumber)), "|l")
                .Replace(char.ConvertFromUtf32(int.Parse("2029", NumberStyles.HexNumber)), "|p")
                .Replace("[", "|[")
                .Replace("]", "|]");
        }
    }
}

