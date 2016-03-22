// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk.Compiler;
using NBench.Sdk;

namespace NBench.Runner
{
    class Program
    {
		/// <summary>
		/// NBench Runner takes the following <see cref="args"/>
		/// 
		/// C:\> NBench.Runner.exe [assembly name] [output-directory={dir-path}] [configuration={file-path}] [include=MyTest*.Perf*,Other*Spec] [exclude=*Long*]
		/// 
		/// </summary>
		/// <param name="args">The commandline arguments</param>
		static int Main(string[] args)
		{
			string[] include = null;
			string[] exclude = null;
			if (CommandLine.HasProperty("include"))
				include = CommandLine.GetProperty("include").Split(',');
			if (CommandLine.HasProperty("exclude"))
				include = CommandLine.GetProperty("exclude").Split(',');

			TestPackage package = new TestPackage(CommandLine.GetFiles(args), include, exclude);

			if (CommandLine.HasProperty("output-directory"))
				package.OutputDirectory = CommandLine.GetProperty("output-directory");

			if (CommandLine.HasProperty("configuration"))			
				package.ConfigurationFile = CommandLine.GetProperty("configuration");

			package.Validate();

		    Console.ReadLine(); //BLOCK
            var result = TestRunner.Run(package);
       
            return result.AllTestsPassed ? 0 : -1;
        }
    }
}

