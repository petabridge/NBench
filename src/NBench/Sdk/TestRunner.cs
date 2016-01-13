using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NBench.Sdk
{
    /// <summary>
    /// Executor of tests
    /// </summary>
    /// <remarks>Will be created in separated appDomain therefor it have to be marshaled.</remarks>
    public class TestRunner : MarshalByRefObject
    {
        private TestPackage _package;

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
            var runnerType = typeof(TestRunner);
            return domain.CreateInstanceAndUnwrap(runnerType.Assembly.FullName, runnerType.FullName, false, 0, null, new object[] { package }, null, null) as TestRunner;
        }

        /// <summary>
        /// Executes the test package.
        /// </summary>
        /// <param name="package">The test package to execute.</param>
        /// <returns>True if all tests passed.</returns>
        /// <remarks>Creates a new AppDomain and executes the tests.</remarks>
        public static bool Run(TestPackage package)
        {
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
        public static void SetProcessPriority()
        {
            /*
            * Set processor affinity
            */
            Process Proc = Process.GetCurrentProcess();
            Proc.ProcessorAffinity = new IntPtr(2); // either of the first two processors

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
        public bool Execute()
        {
            SetProcessPriority();

            var output = new CompositeBenchmarkOutput(new ConsoleBenchmarkOutput(), new MarkdownBenchmarkOutput(_package.OutputDirectory));
            var discovery = new ReflectionDiscovery(output);
            bool allTestsPassed = true;

            foreach (var testFile in _package.Files)
            {
                var assembly = AssemblyRuntimeLoader.LoadAssembly(testFile);

                var benchmarks = discovery.FindBenchmarks(assembly);

                foreach (var benchmark in benchmarks)
                {
                    output.WriteLine($"------------ STARTING {benchmark.BenchmarkName} ---------- ");
                    benchmark.Run();
                    benchmark.Finish();

                    // if one assert fails, all fail
                    allTestsPassed = allTestsPassed && benchmark.AllAssertsPassed;
                    output.WriteLine($"------------ FINISHED {benchmark.BenchmarkName} ---------- ");
                }
            }

            return allTestsPassed;
        }

        /// <summary>
        /// Control the lifetime policy for this instance
        /// </summary>
        public override object InitializeLifetimeService()
        {
            // Live forever
            return null;
        }
    }
}
