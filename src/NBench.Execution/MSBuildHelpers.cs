using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NBench
{
    /// <summary>
    /// INTERNAL API.
    /// 
    /// Used for manipulating MSBUILD15 data needed by the CLI tool and others.
    /// </summary>
    internal static class MsBuildHelpers
    {

#if CORECLR // only used by the DotNetCli tool
        public static string MsBuildName { get; }

        static MsBuildHelpers()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                MsBuildName = "MSBuild.exe";
            else
                MsBuildName = "msbuild";
        }
#endif

        /// <summary>
        /// Parses all of the supported target frameworks for a given MSBuild15 project
        /// </summary>
        /// <param name="projectPath">The path to the `.csproj` file.</param>
        /// <returns>A list of frameworks supported by this project</returns>
        public static string[] GetTargetFrameworks(string projectPath)
        {
            Debug.Assert(projectPath != null);
            var doc = new XmlDocument();
#if CORECLR
            using (var stream = File.Open(projectPath, FileMode.Open))
            {
                doc.Load(stream);
                return GetTargetFrameworks(doc);
            }
#else
            doc.Load(projectPath);
            return GetTargetFrameworks(doc);
#endif

        }

        public static string[] GetTargetFrameworks(XmlDocument doc)
        {
            var frameworkNode = doc.DocumentElement.SelectSingleNode("/Project/PropertyGroup[1]/TargetFrameworks/text()")?.Value 
                ?? doc.DocumentElement.SelectSingleNode("/Project/PropertyGroup[1]/TargetFramework/text()")?.Value;
            
            return frameworkNode.Split(';');
        }
    }
}
