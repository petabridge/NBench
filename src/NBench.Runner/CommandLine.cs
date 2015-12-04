// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Specialized;

namespace NBench.Runner
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
        private readonly static Lazy<StringDictionary> Values = new Lazy<StringDictionary>(() =>
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

        public static string GetProperty(string key)
        {
            return Values.Value[key];
        }

        public static int GetInt32(string key)
        {
            return Convert.ToInt32(GetProperty(key));
        }
    }
}

