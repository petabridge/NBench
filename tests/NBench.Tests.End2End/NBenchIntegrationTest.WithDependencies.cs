using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace NBench.Tests.End2End
{

    public class NBenchIntregrationTestWithDependenciesLoadAssembly
    {
        private readonly ITestOutputHelper _output;

        public NBenchIntregrationTestWithDependenciesLoadAssembly(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact()]
        public void LoadAssemblyCorrect()
        {
            if (!TestRunner.IsMono) // this test doesn't pass yet on Mono
            {
                var package = LoadPackageWithDependencies();
                var result = TestRunner.Run(package);
                result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
                result.ExecutedTestsCount.Should().NotBe(0);
                result.IgnoredTestsCount.Should().Be(0);
            }
        }

        private static TestPackage LoadPackageWithDependencies(IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
#if CORECLR
		    var assemblySubfolder = "netstandard1.6";
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
