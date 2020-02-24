// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using IBenchmarkOutput = NBench.Reporting.IBenchmarkOutput;

namespace NBench.Sdk
{
    /// <summary>
    /// Results collected by the test runner
    /// </summary>
    public class TestRunnerResult
    {
        public bool AllTestsPassed { get; set; }

        public int ExecutedTestsCount { get; set; }
        public int IgnoredTestsCount { get; set; }

        public IReadOnlyList<BenchmarkFinalResults> FullResults { get; set; }
    }
    /// <summary>
    /// Executor of tests
    /// </summary>
    /// <remarks>Will be created in separated appDomain therefor it have to be marshaled.</remarks>
    public class TestRunner
    {
        /// <summary>
        /// Can't apply some of our optimization tricks if running Mono, due to need for elevated permissions
        /// </summary>
        public static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;

        private readonly TestPackage _package;

        private readonly List<BenchmarkFinalResults> _results = new List<BenchmarkFinalResults>();

        private IBenchmarkOutput _resultsCollector;

        /// <summary>
        /// Initializes a new instance of the test runner.
        /// </summary>
        /// <param name="package">The test package to be executed</param>
        public TestRunner(TestPackage package)
        {
            _package = package;
            _resultsCollector = new ActionBenchmarkOutput(benchmarkAction: f => { _results.Add(f); });
        }

        /// <summary>
        /// Executes the test package.
        /// </summary>
        /// <param name="package">The test package to execute.</param>
        /// <remarks>Creates a new instance of <see cref="TestRunner"/> and executes the tests.</remarks>
        public static TestRunnerResult Run(TestPackage package)
        {
            Contract.Requires(package != null);
            var runner = new TestRunner(package);
            return runner.Execute();
        }

        /// <summary>
        /// Initializes the process and thread
        /// </summary>
        public static void SetProcessPriority(bool concurrent)
        {
            /*
            * Set processor affinity
            */
            if (!concurrent)
            {
                var proc = Process.GetCurrentProcess();
                proc.ProcessorAffinity = new IntPtr(2); // strictly the second processor!
            }

            /*
             * Set priority
             */
            if (!IsMono)
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            if (!concurrent)
            {
                /*
                 * If we're running in concurrent mode, don't give the foreground thread higher priority
                 * over the other threads participating in NBench specs. Treat them all equally with the same
                 * priority.
                 */
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }
        }

        /// <summary>
        /// Executes the tests
        /// </summary>
        /// <returns>True if all tests passed.</returns>
        public TestRunnerResult Execute()
        {
            // Perform core / thread optimizations if we're running in single-threaded mode
            // But not if the user has specified that they're going to be running multi-threaded benchmarks
            SetProcessPriority(_package.Concurrent);

            // pass in the runner settings so we can include them in benchmark reports
            // also, toggles tracing on or off
            var runnerSettings = new RunnerSettings()
            {
                ConcurrentModeEnabled = _package.Concurrent,
                TracingEnabled = _package.Tracing
            };

            IBenchmarkOutput output = CreateOutput();


            var discovery = new ReflectionDiscovery(output,
                DefaultBenchmarkAssertionRunner.Instance, // one day we might be able to pass in custom assertion runners, hence why this is here
                runnerSettings);
            var result = new TestRunnerResult()
            {
                AllTestsPassed = true,
                FullResults = _results
            };

            try
            {
                foreach (var assembly in _package.TestAssemblies)
                {
                    output.WriteLine($"Executing Benchmarks in {assembly}");
                    var benchmarks = discovery.FindBenchmarks(assembly);

                    foreach (var benchmark in benchmarks)
                    {
                        // verify if the benchmark should be included/excluded from the list of benchmarks to be run
                        if (_package.ShouldRunBenchmark(benchmark.BenchmarkName))
                        {
                            output.StartBenchmark(benchmark.BenchmarkName);
                            benchmark.Run();
                            benchmark.Finish();

                            // if one assert fails, all fail
                            result.AllTestsPassed = result.AllTestsPassed && benchmark.AllAssertsPassed;
                            output.FinishBenchmark(benchmark.BenchmarkName);
                            result.ExecutedTestsCount = result.ExecutedTestsCount + 1;
                        }
                        else
                        {
                            output.SkipBenchmark(benchmark.BenchmarkName);
                            result.IgnoredTestsCount = result.IgnoredTestsCount + 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                output.Error(ex, "Error while executing the tests.");
                result.AllTestsPassed = false;
            }

            return result;
        }

        /// <summary>
        /// Creates the benchmark output writer
        /// </summary>
        /// <returns></returns>
        protected virtual IBenchmarkOutput CreateOutput()
        {
            var outputs = new List<IBenchmarkOutput>() { _resultsCollector };
            var consoleOutput = _package.TeamCity ?
                new TeamCityBenchmarkOutput()
                : (IBenchmarkOutput)new ConsoleBenchmarkOutput();
            outputs.Add(consoleOutput);
            if (!string.IsNullOrEmpty(_package.OutputDirectory))
            {
                outputs.Add(new MarkdownBenchmarkOutput(_package.OutputDirectory));
            }

            return new CompositeBenchmarkOutput(outputs.ToArray());
        }
    }
}

