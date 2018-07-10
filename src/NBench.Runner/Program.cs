// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using NBench.Sdk;

namespace NBench.Runner
{
    class Program
    {
		/// <summary>
		/// NBench Runner takes the following <see cref="args"/>
		/// 
		/// C:\> NBench.Runner.exe [assembly name] [output-directory={dir-path}] [configuration={file-path}] [include=MyTest*.Perf*,Other*Spec] [exclude=*Long*] [concurrent={true|false}] [trace={true|false}] [teamcity={true|false}]
		/// 
		/// </summary>
		/// <param name="args">The commandline arguments</param>
		static int Main(string[] args)
		{
			string[] include = null;
			string[] exclude = null;
		    bool concurrent = false;
		    bool trace = false;
		    bool teamcity = false;

		    if (args.Length == 1 && args[0] == "--help")
		    {
		        CommandLine.ShowHelp();
		        return 0;
		    }

		    if (CommandLine.HasProperty(CommandLine.DiagnosticsKey))
		    {
		        Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("DIAG: Executing with parameters [{0}]", CommandLine.FormatCapturedArguments());
                Console.WriteLine("DIAG: Unparsed arguments [{0}]", string.Join(",", Environment.GetCommandLineArgs()));
                Console.WriteLine($"DIAG: Captured, but unrecognized arguments: {string.Join(",", CommandLine.Values.Value.Select(x => $"{x.Key}:[{string.Join(",", x.Value)}]"))}");
                Console.ResetColor();
		    }

			if (CommandLine.HasProperty("include"))
				include = CommandLine.GetProperty("include")?.ToArray();
			if (CommandLine.HasProperty("exclude"))
				exclude = CommandLine.GetProperty("exclude")?.ToArray();
		    if (CommandLine.HasProperty("concurrent"))
		        concurrent = CommandLine.GetBool("concurrent");
		    if (CommandLine.HasProperty("trace"))
		        trace = CommandLine.GetBool("trace");
		    if (CommandLine.HasProperty("teamcity"))
		        teamcity = CommandLine.GetBool("teamcity");
		    else
		    {
                // try to auto-detect if not explicitly set
                teamcity = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME"));
            }

		    var files = CommandLine.GetFiles(args);
		    if (files.Count == 0)
		    {
                Console.WriteLine("Please provide assemblies for which to run NBench tests\n");
		        CommandLine.ShowHelp();
		        return 1;
		    }

		    TestPackage package =
		        new TestPackage(files, include, exclude, concurrent) {Tracing = trace};

		    if (CommandLine.HasProperty(CommandLine.OutputKey))
		        package.OutputDirectory = CommandLine.GetSingle(CommandLine.OutputKey);

		    if (CommandLine.HasProperty(CommandLine.ConfigurationKey))
		        package.ConfigurationFile = CommandLine.GetSingle(CommandLine.ConfigurationKey);

		    package.TeamCity = teamcity;

		    package.Validate();
		    var result = TestRunner.Run(package);

		    return result.AllTestsPassed ? 0 : -1;
		}
    }
}

