using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBench.Sdk;
using Xunit;

namespace NBench.Tests.End2End.Reporting
{
    public class MarkdownBenchmarkOutputSpec
    {
        [Fact]
        public void LoadAssemblyCorrect()
        {
            var package = LoadPackage();
            package.OutputDirectory = "../../";
            var result = TestRunner.Run(package);
        }

        private static TestPackage LoadPackage(IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
#if CORECLR
		    var assemblySubfolder = "netcoreapp1.0";
#else
            var assemblySubfolder = "net452";
#endif

#if DEBUG
            var package = new TestPackage(".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "NBench.Tests.Assembly" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Debug" + Path.DirectorySeparatorChar + assemblySubfolder + Path.DirectorySeparatorChar + "NBench.Tests.Assembly.dll", include, exclude);
#else
            var package = new TestPackage(".." + Path.DirectorySeparatorChar + ".."+ Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "NBench.Tests.Assembly" + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "Release" + Path.DirectorySeparatorChar + assemblySubfolder + Path.DirectorySeparatorChar + "NBench.Tests.Assembly.dll", include, exclude);
#endif

            package.Validate();
            return package;
        }
    }
}
