using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using Xunit;

namespace NBench.Tests.End2End
{
    public class NBenchIntregrationTestWithDependencies
    {
        private static readonly IBenchmarkOutput _benchmarkOutput = new ActionBenchmarkOutput(report => { }, results =>
        {
            foreach (var assertion in results.AssertionResults)
            {
                Assert.True(assertion.Passed, results.BenchmarkName + " " + assertion.Message);
            }
        });

        private readonly IDiscovery _discovery = new ReflectionDiscovery(_benchmarkOutput);

        [Fact]
        public void ShouldPassAllBenchmarks()
        {
            if (!TestRunner.IsMono) // this spec currently hits a runtime exception with Mono: https://bugzilla.xamarin.com/show_bug.cgi?id=43291
            {
                var benchmarks = _discovery.FindBenchmarks(GetType().GetTypeInfo().Assembly).ToList();
                Assert.True(benchmarks.Count >= 1);
                Benchmark.PrepareForRun(); // force some GC here
                for (var i = 0; i < benchmarks.Count; i++)
                {
                    Benchmark.PrepareForRun(); // force some GC here
                    benchmarks[i].Run();
                    benchmarks[i].Finish();
                }
            }
        }

        [Fact]
        public void LoadAssemblyCorrect()
        {
            var package = LoadPackage();
            var result = TestRunner.Run(package);
            Assert.True(result.AllTestsPassed);
            Assert.NotEqual(0, result.ExecutedTestsCount);
            Assert.Equal(0, result.IgnoredTestsCount);
        }

        private static TestPackage LoadPackage(IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
#if CORECLR
		    var assemblySubfolder = "netcoreapp1.1";
#else
            var assemblySubfolder = "net452";
#endif

#if DEBUG
            var package = new TestPackage(".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "NBench.Tests.Performance.WithDependencies" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar + assemblySubfolder + Path.DirectorySeparatorChar + "NBench.Tests.Performance.WithDependencies.dll", include, exclude);
#else
            var package = new TestPackage(".." + Path.DirectorySeparatorChar + ".."+ Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "NBench.Tests.Performance.WithDependencies" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar + assemblySubfolder + Path.DirectorySeparatorChar + "NBench.Tests.Performance.WithDependencies.dll", include, exclude);
#endif

            package.Validate();
            return package;
        }
    }
}
