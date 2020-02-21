using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBench.Sdk;

namespace NBench
{
    public static class NBenchRunner
    {
        public static TestPackage CreateTest<TType>()
        {
			string[] include = null;
			string[] exclude = null;
			var concurrent = false;
			var trace = false;
			var teamcity = false;

			if (NBenchCommands.HasProperty(NBenchCommands.DiagnosticsKey))
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("DIAG: Executing with parameters [{0}]", NBenchCommands.FormatCapturedArguments());
				Console.WriteLine("DIAG: Unparsed arguments [{0}]", string.Join(",", Environment.GetCommandLineArgs()));
				Console.WriteLine($"DIAG: Captured, but unrecognized arguments: {string.Join(",", NBenchCommands.Values.Value.Select(x => $"{x.Key}:[{string.Join(",", x.Value)}]"))}");
				Console.ResetColor();
			}

			if (NBenchCommands.HasProperty(NBenchCommands.IncludeKey))
				include = NBenchCommands.GetProperty(NBenchCommands.IncludeKey)?.ToArray();
			if (NBenchCommands.HasProperty(NBenchCommands.ExcludeKey))
				exclude = NBenchCommands.GetProperty(NBenchCommands.ExcludeKey)?.ToArray();
			if (NBenchCommands.HasProperty(NBenchCommands.ConcurrentKey))
				concurrent = NBenchCommands.GetBool(NBenchCommands.ConcurrentKey);
			if (NBenchCommands.HasProperty(NBenchCommands.TracingKey))
				trace = NBenchCommands.GetBool(NBenchCommands.TracingKey);
			if (NBenchCommands.HasProperty(NBenchCommands.TeamCityKey))
				teamcity = true;
			else
			{
				// try to auto-detect if not explicitly set
				teamcity = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME"));
			}

			var files = typeof(TType).Assembly;

			TestPackage package =
				new TestPackage(files, include, exclude, concurrent) { Tracing = trace };

			if (NBenchCommands.HasProperty(NBenchCommands.OutputKey))
				package.OutputDirectory = NBenchCommands.GetSingle(NBenchCommands.OutputKey);

			if (NBenchCommands.HasProperty(NBenchCommands.ConfigurationKey))
				package.ConfigurationFile = NBenchCommands.GetSingle(NBenchCommands.ConfigurationKey);

			package.TeamCity = teamcity;

            return package;
        }

		/// <summary>
		/// Prints help messages to the console.
		/// </summary>
        public static void PrintHelp()
        {
			NBenchCommands.ShowHelp();
        }

        public static int Run<TType>()
        {
            var package = CreateTest<TType>();
			var result = TestRunner.Run(package);

			return result.AllTestsPassed ? 0 : -1;
		}
    }
}
