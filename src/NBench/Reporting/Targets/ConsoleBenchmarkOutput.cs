// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench.Reporting.Targets
{
    /// <summary>
    /// Output writer to the console for NBench
    /// </summary>
    public class ConsoleBenchmarkOutput : IBenchmarkOutput
    {
        public void WriteStartingBenchmark(string benchmarkName)
        {
            Console.WriteLine("--------------- STARTING {0} ---------------", benchmarkName);
        }

        public void WriteRun(BenchmarkRunReport report, bool isWarmup = false)
        {
            if (isWarmup)
                Console.WriteLine("--------------- BEGIN WARMUP ---------------");
            else
            {
                Console.WriteLine("--------------- BEGIN RUN ---------------");
            }
           
            Console.WriteLine("Elapsed: {0}", report.Elapsed);
            foreach (var metric in report.Metrics.Values)
                Console.WriteLine("{0}: Max: {2} {1}, Average: {3} {1}, Min: {4} {1}, StdDev: {5} {1}", metric.Name,
                    metric.Unit, metric.Stats.Max, metric.Stats.Mean, metric.Stats.Min, metric.Stats.StandardDeviation);
           
            
            if (isWarmup)
                Console.WriteLine("--------------- END WARMUP ---------------");
            else
            {
                Console.WriteLine("--------------- END RUN ---------------");
            }
            Console.WriteLine();
        }

        public void WriteBenchmark(BenchmarkFinalResults results)
        {
            Console.WriteLine("--------------- RESULTS: {0} ---------------", results.BenchmarkName);

            Console.WriteLine("--------------- DATA ---------------");
            foreach (var metric in results.Data.StatsByMetric.Values)
            {
                Console.WriteLine("{0}: Max: {2} {1}, Average: {3} {1}, Min: {4} {1}, StdDev: {5} {1}", metric.Name,
                    metric.Unit, metric.Maxes.Max, metric.Averages.Mean, metric.Mins.Min, metric.StandardDeviations.StandardDeviation);

                Console.WriteLine("{0}: Max / s {2} {1}, Average / s: {3} {1}, Min / s: {4} {1}, StdDev / s: {5} {1}", metric.Name,
                    metric.Unit, metric.PerSecondMaxes.Max, metric.PerSecondAverages.Mean, metric.PerSecondMins.Min, metric.PerSecondStandardDeviations.StandardDeviation);
                Console.WriteLine();
            }

            Console.WriteLine("--------------- ASSERTIONS ---------------");
            foreach(var assertion in results.AssertionResults)
                Console.WriteLine(assertion.Message);

            Console.WriteLine();
        }
    }
}

