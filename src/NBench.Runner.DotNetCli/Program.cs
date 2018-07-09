// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NBench.Sdk;

namespace NBench.Runner.DotNetCli
{
    class Program
    {
        string Configuration;
        string FxVersion;
        bool NoBuild;

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

                // Need to amend the paths for the report output, since we are always running
                // in the context of the bin folder, not the project folder
                var currentDirectory = Directory.GetCurrentDirectory();

            }
            catch (Exception ex)
            {
                return 3;
                Console.WriteLine("Error: {0}", ex.Message);
            }

            return 0;
        }

    }
}

