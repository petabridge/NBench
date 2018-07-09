using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <summary>
        /// Parses all of the supported target frameworks for a given MSBuild15 project
        /// </summary>
        /// <param name="projectPath">The path to the `.csproj` file.</param>
        /// <returns>A list of frameworks supported by this project</returns>
        public static string[] GetTargetFrameworks(string projectPath)
        {
            Debug.Assert(projectPath != null);
            var doc = XDocument.Load(projectPath);
            return GetTargetFrameworks(doc);
        }

        public static string[] GetTargetFrameworks(XDocument doc)
        {
            var frameworkNode = doc.Element("TargetFrameworks")?.Value ?? doc.Element("TargetFramework")?.Value;
            Debug.Assert(frameworkNode != null);
            return frameworkNode.Split(';');
        }
    }
}
