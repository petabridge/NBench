// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
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
			if (CommandLine.HasProperty("include"))
				include = CommandLine.GetProperty("include").Split(',');
			if (CommandLine.HasProperty("exclude"))
				exclude = CommandLine.GetProperty("exclude").Split(',');
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


            TestPackage package = new TestPackage(CommandLine.GetFiles(args), include, exclude, concurrent) {Tracing = trace};

			if (CommandLine.HasProperty("output-directory"))
				package.OutputDirectory = CommandLine.GetProperty("output-directory");

			if (CommandLine.HasProperty("configuration"))			
				package.ConfigurationFile = CommandLine.GetProperty("configuration");

		    package.TeamCity = teamcity;

			package.Validate();
            var result = TestRunner.Run(package);
       
            return result.AllTestsPassed ? 0 : -1;
        }
    }
}

