// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// Inspired by https://github.com/xunit/xunit/blob/master/src/dotnet-xunit/Program.cs

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NBench.Sdk;
using static NBench.MsBuildHelpers;

namespace NBench.Runner.DotNetCli
{
    class Program
    {
        string Configuration;
        string FxVersion;
        bool NoBuild;
        private string ThisAssemblyPath;
        string BuildStdProps;

        /// <summary>
        /// NBench Runner takes the following <see cref="args"/>
        /// 
        /// C:\> dotnet nbench [assembly name] [output-directory={dir-path}] [configuration={file-path}] [include=MyTest*.Perf*,Other*Spec] [exclude=*Long*] [concurrent={true|false}] [trace={true|false}] [teamcity={true|false}]
        /// 
        /// </summary>
        /// <param name="args">The commandline arguments</param>
        public static int Main(string[] args)
        {
            return new Program().Execute(args);
        }

        int Execute(string[] args)
        {
            // Let Ctrl+C pass down into the child processes, ignoring it here
            Console.CancelKeyPress += (sender, e) => e.Cancel = true;

            try
            {
                if (args.Length == 1 && args[0] == "--help")
                {
                    CommandLine.ShowHelp();
                    return 0;
                }

                string requestedTargetFramework;

                var files = CommandLine.GetFiles(args);
                if (files.Count == 0)
                {
                    Console.WriteLine("Please provide assemblies for which to run NBench tests\n");
                    CommandLine.ShowHelp();
                    return 1;
                }

                // The extra versions are unadvertised compatibility flags to match 'dotnet' command line switches
                requestedTargetFramework = (CommandLine.GetProperty("-framework")
                                            ?? CommandLine.GetProperty("--framework")
                                            ?? CommandLine.GetProperty("-f")).SingleOrDefault();
                Configuration = (CommandLine.GetProperty("-configuration")
                                ?? CommandLine.GetProperty("--configuration")
                                ?? CommandLine.GetProperty("-c")).SingleOrDefault()
                                ?? "Release";
                FxVersion = (CommandLine.GetProperty("-fxversion")
                            ?? CommandLine.GetProperty("--fx-version")).SingleOrDefault();
                NoBuild = (CommandLine.HasProperty("-nobuild")
                          || CommandLine.HasProperty("--no-build"));

                var testProjects = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*proj")
                    .Where(x => !x.EndsWith(".xproj")) // skip the beta .NET Core format
                    .ToList();

                if (testProjects.Count == 0)
                {
                    WriteLineError("Could not find any project (*.*proj) file in the current directory.");
                    return 3;
                }

                if (testProjects.Count > 1)
                {
                    WriteLineError($"Multiple project files were found; only a single project file is supported. Found: {string.Join(", ", testProjects.Select(x => Path.GetFileName(x)))}");
                    return 3;
                }

                ThisAssemblyPath = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);
                BuildStdProps = $"\"/p:_NBench_ImportTargetsFile={Path.Combine(ThisAssemblyPath, "import.targets")}\" " +
                                $"/p:Configuration={Configuration}";

                var testProject = testProjects[0];

                var targetFrameworks = GetTargetFrameworks(testProject);
                //if(targetFrameworks == null)

            }
            catch (Exception ex)
            {
                WriteLineError($"Error: {ex.Message}");
                return 3;
            }

           
            return 0;
        }

        public void WriteLineError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}

