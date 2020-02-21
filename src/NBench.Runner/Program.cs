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
			

		    if (args.Length == 1 && args[0] == "--help")
		    {
		        NBenchCommands.ShowHelp();
		        return 0;
		    }

           
		}
    }
}

