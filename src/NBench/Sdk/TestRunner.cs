// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk.Compiler;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if CORECLR
using System.Runtime.Loader;
#endif
using System.Threading;
using Microsoft.Win32;

namespace NBench.Sdk
{
    /// <summary>
    /// Results collected by the test runner
    /// </summary>
#if !CORECLR
    [Serializable]
#endif
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
    public class TestRunner
#if !CORECLR
        : MarshalByRefObject
#endif
    {
        /// <summary>
        /// Can't apply some of our optimization tricks if running Mono, due to need for elevated permissions
        /// </summary>
        public static readonly bool IsMono = Type.GetType("Mono.Runtime") != null;

        private readonly TestPackage _package;

        /// <summary>
        /// Initializes a new instance of the test runner.
        /// </summary>
        /// <param name="package">The test package to be executed</param>
        public TestRunner(TestPackage package)
        {
            _package = package;
        }


#if !CORECLR
        /// <summary>
        /// Creates a new instance of the test runner in the given app domain.
        /// </summary>
        /// <param name="domain">The app domain to create the runner into.</param>
        /// <param name="package">The test package to execute.</param>
        /// <returns></returns>
        public static TestRunner CreateRunner(System.AppDomain domain, TestPackage package)
        {
            Contract.Requires(domain != null);
            var runnerType = typeof(TestRunner);
            return domain.CreateInstanceAndUnwrap(runnerType.Assembly.FullName, runnerType.FullName, false, 0, null, new object[] { package }, null, null) as TestRunner;
        }
#else
        /// <summary>
        /// Creates a new instance of the test runner in the given app domain.
        /// </summary>
        /// <param name="package">The test package to execute.</param>
        /// <returns></returns>
        public static TestRunner CreateRunner(TestPackage package)
        {
            return (TestRunner) Activator.CreateInstance(typeof(TestRunner), new object[] {package});
        }
#endif

#if !CORECLR
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
#else

        /// <summary>
        /// Executes the test package.
        /// </summary>
        /// <param name="package">The test package to execute.</param>
        /// <remarks>Creates a new instance of <see cref="TestRunner"/> and executes the tests.</remarks>
        public static TestRunnerResult Run(TestPackage package)
        {
            Contract.Requires(package != null);
            var runner = TestRunner.CreateRunner(package);
            return runner.Execute();
        }
#endif

        /// <summary>
        /// Initializes the process and thread
        /// </summary>
        public void SetProcessPriority(bool concurrent)
        {
#if !CORECLR
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
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
                if (!concurrent)
                {
                    /*
                     * If we're running in concurrent mode, don't give the foreground thread higher priority
                     * over the other threads participating in NBench specs. Treat them all equally with the same
                     * priority.
                     */
                    //Thread.CurrentThread.Priority = ThreadPriority.Highest;
                }
            }
#endif
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
                AllTestsPassed = true
            };

            try
            {
                foreach (var testFile in _package.Files)
                {
                    output.WriteLine($"Executing Benchmarks in {testFile}");
                    using (var assembly = AssemblyRuntimeLoader.LoadAssembly(testFile, output))
                    {
                        var benchmarks = discovery.FindBenchmarks(assembly.Assembly);

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
            }
            catch (Exception ex)
            {
                output.Error(ex, "Error while executing the tests.");
                result.AllTestsPassed = false;
            }

            return result;
        }

#if !CORECLR
        /// <summary>
        /// Control the lifetime policy for this instance
        /// </summary>
        public override object InitializeLifetimeService()
        {
            // Live forever
            return null;
        }
#endif

        /// <summary>
        /// Creates the benchmark output writer
        /// </summary>
        /// <returns></returns>
        protected virtual IBenchmarkOutput CreateOutput()
        {
            var consoleOutput = _package.TeamCity ? 
                new TeamCityBenchmarkOutput() 
                : (IBenchmarkOutput)new ConsoleBenchmarkOutput(); 
            if (string.IsNullOrEmpty(_package.OutputDirectory))
                return consoleOutput;
            else
                return new CompositeBenchmarkOutput(consoleOutput, new MarkdownBenchmarkOutput(_package.OutputDirectory));
        }
    }
}

