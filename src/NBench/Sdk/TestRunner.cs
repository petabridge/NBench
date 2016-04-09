// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk.Compiler;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;

namespace NBench.Sdk
{
    /// <summary>
    /// Results collected by the test runner
    /// </summary>
    [Serializable]
    public class TestRunnerResult
    {
        public bool AllTestsPassed { get; set; }

        public int ExecutedTestsCount { get; set; }
        public int IgnoredTestsCount { get; set; }
    }
    /// <summary>
    /// Executor of tests
    /// </summary>
    /// <remarks>Will be created in separated appDomain therefor it have to be marshaled.</remarks>
    public class TestRunner : MarshalByRefObject
    {
        private readonly TestPackage _package;

        /// <summary>
        /// Initializes a new instance of the test runner.
        /// </summary>
        /// <param name="package">The test package to be executed</param>
        public TestRunner(TestPackage package)
        {
            _package = package;
        }

        /// <summary>
        /// Creates a new instance of the test runner in the given app domain.
        /// </summary>
        /// <param name="domain">The app domain to create the runner into.</param>
        /// <param name="package">The test package to execute.</param>
        /// <returns></returns>
        public static TestRunner CreateRunner(AppDomain domain, TestPackage package)
        {
            Contract.Requires(domain != null);
            var runnerType = typeof(TestRunner);
            return domain.CreateInstanceAndUnwrap(runnerType.Assembly.FullName, runnerType.FullName, false, 0, null, new object[] { package }, null, null) as TestRunner;
        }

        /// <summary>
        /// Executes the test package.
        /// </summary>
        /// <param name="package">The test package to execute.</param>
        /// <returns>True if all tests passed.</returns>
        /// <remarks>Creates a new AppDomain and executes the tests.</remarks>
        public static TestRunnerResult Run(TestPackage package)
        {
            Contract.Requires(package != null);
            // create the test app domain
            var testDomain = DomainManager.CreateDomain(package);

            try
            {
                var runner = TestRunner.CreateRunner(testDomain, package);
                return runner.Execute();
            }
            finally
            {
                DomainManager.UnloadDomain(testDomain);
            }
        }

        /// <summary>
        /// Initializes the process and thread
        /// </summary>
        public void SetProcessPriority(bool concurrent)
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
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
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

            IBenchmarkOutput output = CreateOutput();
            var discovery = new ReflectionDiscovery(output);
            var result = new TestRunnerResult()
            {
                AllTestsPassed = true
            };

            try
            {
                foreach (var testFile in _package.Files)
                {
                    var assembly = AssemblyRuntimeLoader.LoadAssembly(testFile);

                    var benchmarks = discovery.FindBenchmarks(assembly);

                    foreach (var benchmark in benchmarks)
                    {
                        // verify if the benchmark should be included/excluded from the list of benchmarks to be run
                        if (_package.ShouldRunBenchmark(benchmark.BenchmarkName))
                        {
                            output.WriteLine($"------------ STARTING {benchmark.BenchmarkName} ---------- ");
                            benchmark.Run();
                            benchmark.Finish();

                            // if one assert fails, all fail
                            result.AllTestsPassed = result.AllTestsPassed && benchmark.AllAssertsPassed;
                            output.WriteLine($"------------ FINISHED {benchmark.BenchmarkName} ---------- ");
                            result.ExecutedTestsCount = result.ExecutedTestsCount + 1;
                        }
                        else
                        {
                            output.WriteLine($"------------ NOTRUN {benchmark.BenchmarkName} ---------- ");
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
        /// Control the lifetime policy for this instance
        /// </summary>
        public override object InitializeLifetimeService()
        {
            // Live forever
            return null;
        }

        /// <summary>
        /// Creates the benchmark output writer
        /// </summary>
        /// <returns></returns>
        protected virtual IBenchmarkOutput CreateOutput()
        {
            if (string.IsNullOrEmpty(_package.OutputDirectory))
                return new ConsoleBenchmarkOutput();
            else
                return new CompositeBenchmarkOutput(new ConsoleBenchmarkOutput(), new MarkdownBenchmarkOutput(_package.OutputDirectory));
        }
    }
}

