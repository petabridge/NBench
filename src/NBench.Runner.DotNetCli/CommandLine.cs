// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace NBench.Runner.DotNetCli
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
        private static readonly Lazy<StringDictionary> Values = new Lazy<StringDictionary>(() =>
        {
            var dictionary = new StringDictionary();
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (!arg.Contains("=")) continue;
                var tokens = arg.Split('=');
                dictionary.Add(tokens[0], tokens[1]);
            }
            return dictionary;
        });

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

        public static string GetProperty(string key)
        {
            return Values.Value[key];
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
            return Convert.ToInt32(GetProperty(key));
        }

        public static bool GetBool(string key)
        {
            return Convert.ToBoolean(GetProperty(key));
        }
    }
}

