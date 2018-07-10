// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NBench
{
    /// <summary>
    /// Command line argument parser for NBench specifications
    /// 
    /// Parses arguments from <see cref="Environment.GetCommandLineArgs"/>.
    /// 
    /// For example (from the Akka.NodeTestRunner source):
    /// <code>
    ///     var outputDirectory = CommandLine.GetInt32("output-directory");
    /// </code>
    /// </summary>
    public class CommandLine
    {
        public static readonly string Version = typeof(CommandLine).GetAssembly().GetName().Version.ToString();

        internal static readonly Lazy<Dictionary<string, List<string>>> Values = new Lazy<Dictionary<string, List<string>>>(ParseValues);

        private static Dictionary<string, List<string>> ParseValues()
        {
            var args = Environment.GetCommandLineArgs();
            return ParseValues(args);
        }

        public static Dictionary<string, List<string>> ParseValues(string[] args)
        {
            var dictionary = new Dictionary<string, List<string>>();
            var idx = 0;
            while (idx < args.Length)
            {
                var arg = args[idx++];
                if (arg.StartsWith("-")) // later argument formats (dotnet-cli compatibility)
                {
                    if (!dictionary.TryGetValue(arg, out var values))
                    {
                        values = new List<string>();
                        dictionary.Add(arg, values);
                    }

                    if (idx < args.Length && !args[idx].StartsWith("-"))
                    {
                        values.Add(args[idx++]);
                    }
                    else
                        values.Add(null);
                }
            }

            return dictionary;
        }


        /// <summary>
        /// Retrieve file names from the command line
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFiles(string[] args)
        {
            List<string> files = new List<string>();

            foreach (var arg in args)
            {
                // stop at options
                if (arg.Contains("="))
                    break;

                var ext = Path.GetExtension(arg);
                if (ext.Equals(".dll", StringComparison.OrdinalIgnoreCase) || ext.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    files.Add(arg);
            }

            return files;
        }

        public static void ShowHelp()
        {
            Console.WriteLine($"NBench Runner ({Version})" + @"

Usage: NBench.Runner.exe [assembly names] [output-directory={dir-path}] [configuration={file-path}] [include=MyTest*.Perf*,Other*Spec] [exclude=*Long*] [concurrent={true|false}]

Options:
    --help  Show help

Arguments:
    [assembly names]            List of assemblies to load and test. Space delimited. Requires .dll
                                or .exe at the end of each assembly name.
    [--output path]     Folder where a Markdown report will be exported.
    [--configuration path]        Folder with a config file to be used when loading the assembly names.
    [--include name test pattern] A comma separted list of wildcard pattern to be mached and 
                                included in the tests.  Default value is * (all).  The test is executed
                                on the complete name of the benchmark Namespace.Class+MethodName.
                                Examples: 
                                    include=""*MyBenchmarkClass*"" (include all benchmarks in MyBenchmarkClass)
                                    include=""*MyBenchmarkClass+MyBenchmark"" (include MyBenchmark in MyBenchmarkClass)
                                    include=""*MyBenchmarkClass*,*MyOtherBenchmarkClass*"" (include all benchmarks
                                            in MyBenchmarkClass and MyOtherBenchmarkClass)
    [--exclude name test pattern] A comma separted list of wildcard pattern to be mached and 
                                excluded in the tests.  Default value is (none).  The test is executed 
                                on the complete name of the benchmarkNamespace.Class+MethodName.
                                Examples: 
                                    exclude=""*MyBenchmarkClass*"" (exclude all benchmarks in MyBenchmarkClass)
                                    exclude=""*MyBenchmarkClass+MyBenchmark"" (exclude MyBenchmark in MyBenchmarkClass)
                                    exclude=""*MyBenchmarkClass*,*MyOtherBenchmarkClass*"" (exclude all benchmarks
                                            in MyBenchmarkClass and MyOtherBenchmarkClass)
    [--concurrent true|false]     Disables thread priority and processor affinity operations for all 
                                benchmarks.  Used only when running multi-threaded benchmarks.  
                                Set to false (single-threaded) by default.
    [--trace true|false]        Turns on trace capture inside the NBench runner.  Will save any 
                                captured messages to all available output targets, including Markdown 
                                reports.  Set to false by default.
    [--diagnostic]              Turns on diagnostic logging.
    [--teamcity]                Turns on TeamCity output formatting

");
        }

        public static List<string> GetProperty(string key)
        {
            return Values.Value.ContainsKey(key) ? Values.Value[key] : null;
        }

        /// <summary>
        /// Determines whether a property was written in the command line
        /// </summary>
        /// <param name="key">Name of the property</param>
        /// <returns></returns>
        public static bool HasProperty(string key)
        {
            return Values.Value.ContainsKey(key);
        }

        public static int GetInt32(string key)
        {
            return Convert.ToInt32(GetProperty(key).SingleOrDefault());
        }

        public static bool GetBool(string key)
        {
            return Convert.ToBoolean(GetProperty(key).SingleOrDefault());
        }

        public static string GetSingle(string key)
        {
            return GetProperty(key).SingleOrDefault();
        }

        public const string TracingKey = "--trace";
        public const string ConcurrentKey = "--concurrent";
        public const string ExcludeKey = "--exclude";
        public const string IncludeKey = "--include";
        public const string ConfigurationKey = "--configuration";
        public const string OutputKey = "--output";
        public const string DiagnosticsKey = "--diagnostic";
        public const string TeamCityKey = "--teamcity";

        public static string FormatCapturedArguments(bool includeOutput = true)
        {
            string output = "";

            if (HasProperty(TracingKey))
                output += $"{TracingKey}={GetSingle(TracingKey)} ";

            if (HasProperty(ConcurrentKey))
            {
                output += $"{ConcurrentKey}={GetSingle(ConcurrentKey)} ";
            }

            if (HasProperty(ExcludeKey))
            {
                output += $"{ExcludeKey}={string.Join(",", GetProperty(ExcludeKey))} ";
            }

            if (HasProperty(IncludeKey))
            {
                output += $"{IncludeKey}={string.Join(",", GetProperty(IncludeKey))} ";
            }

            if (HasProperty(ConfigurationKey))
            {
                output += $"{ConfigurationKey}={GetSingle(ConcurrentKey)} ";
            }

            if (HasProperty(OutputKey) && includeOutput)
            {
                output += $"{OutputKey}={GetSingle(OutputKey)} ";
            }

            if (HasProperty(DiagnosticsKey))
            {
                output += $"{DiagnosticsKey} ";
            }

            return output;
        }
    }
}

