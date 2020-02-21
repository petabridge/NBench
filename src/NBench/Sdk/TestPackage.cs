// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;


namespace NBench.Sdk
{
    /// <summary>
    /// A TestPackage contains one or more test files. It also holds settings how the tests should be loaded. 
    /// </summary>
    public class TestPackage
    {
        /// <summary>
		/// List of patterns to be included in the tests. Wildchars supported (*, ?)
		/// </summary>
		private List<string> _include = new List<string>();
		private List<Regex> _includePatterns;	// locally build cache

		/// <summary>
		/// List of patterns to be excluded from the tests. Wildchars supported (*, ?)
		/// </summary>
		private List<string> _exclude = new List<string>();
		private List<Regex> _excludePatterns;   // locally build cache


		/// <summary>
		/// Gets or sets a file path to the configuration file (app.config) used for the test assemblies
		/// </summary>
		public string ConfigurationFile { get; set; }

        /// <summary>
        /// Gets or sets the directory for the result output file
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the name of the test package
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If <c>true</c>, NBench disables any processor affinity or thread priority settings.
        /// If <c>false</c>, NBench will run in single-threaded mode and set processor affinity + thread priority.
        /// 
        /// Defaults to false.
        /// </summary>
        public bool Concurrent { get; set; }

		/// <summary>
		/// Gets the assemblies containing tests
		/// </summary>
		public IReadOnlyList<Assembly> TestAssemblies { get; }

        /// <summary>
        /// If <c>true</c>, NBench enables tracing and writes its output to all configured output targets.
        /// If <c>false</c>, NBench disables tracing and will not write any output.
        /// 
        /// Defaults to false.
        /// </summary>
        public bool Tracing { get; set; }

        /// <summary>
        /// If <c>true</c>, NBench uses TeamCity formatting for all of its benchmark methods.
        /// If <c>false</c>, NBench uses regular console logging for all of its benchmark methods.
        /// 
        /// Defaults to false.
        /// </summary>
        public bool TeamCity { get; set; }

		/// <summary>
		/// Initializes a new test package with one test assembly.
		/// </summary>
		/// <param name="testAssembly">An assembly to test.</param>
		/// <param name="include">An optional include pattern</param>
		/// <param name="exclude">An optional exclude pattern</param>
		/// <param name="concurrent">Enable benchmarks that use multiple threads. See <see cref="Concurrent"/> for more details.</param>
		public TestPackage(Assembly testAssembly, IEnumerable<string> include = null, IEnumerable<string> exclude = null, bool concurrent = false)
		{
			if (testAssembly == null)
				throw new ArgumentNullException(nameof(testAssembly));

			TestAssemblies = new List<Assembly>(){ testAssembly };

			if (include != null)
			{
				foreach (var p in include)
					AddInclude(p.Trim());
			}
			if (exclude != null)
			{
				foreach (var p in exclude)
					AddExclude(p.Trim());
			}

            Concurrent = concurrent;
		}

        /// <summary>
        /// Initializes a new package with multiple test files.
        /// </summary>
        /// <param name="assemblies">A list of test files.</param>
        /// <param name="include">Optional list of include patterns</param>
        /// <param name="exclude">Optional list of exclude patterns</param>
        /// <param name="concurrent">Enable benchmarks that use multiple threads. See <see cref="Concurrent"/> for more details.</param>
        public TestPackage(IEnumerable<Assembly> assemblies, IEnumerable<string> include = null, IEnumerable<string> exclude = null, bool concurrent = false)
        {
            var enumerable = assemblies.ToArray();
            if (assemblies == null || !enumerable.Any())
                throw new ArgumentException("Please provide at least one test assembly." ,nameof(assemblies));

			// if only one file is given use same common logic
            TestAssemblies = enumerable;

		    if (include != null)
		    {
			    foreach(var p in include)
					AddInclude(p.Trim());
		    }
			if (exclude != null)
			{
				foreach (var p in exclude)
					AddExclude(p.Trim());
			}

            Concurrent = concurrent;
        }

		/// <summary>
		/// Add a pattern to be excluded. We'll ignore nulls.
		/// </summary>
		/// <param name="exclude"></param>
		public void AddExclude(string exclude)
		{
			if (String.IsNullOrEmpty(exclude))
				return;
			
			_exclude.Add(exclude);
		}

		/// <summary>
		/// Add a pattern to be included. We'll ignore nulls.
		/// </summary>
		/// <param name="include"></param>
		public void AddInclude(string include)
		{
			if (String.IsNullOrEmpty(include))
				return;
			_include.Add(include);
		}

	    public bool ShouldRunBenchmark(string benchmarkName)
	    {
		    PreparePatterns();
		    return _includePatterns.Any(p => p.IsMatch(benchmarkName)) && !_excludePatterns.Any(p => p.IsMatch(benchmarkName));
	    }

	    private void PreparePatterns()
	    {
		    if (_includePatterns != null)
			    return;
			_includePatterns = new List<Regex>();
		    if (_include.Count == 0)
		    {
			    // no pattern - assume pattern is "*"
			    _includePatterns.Add(BuildPattern("*"));
		    }
		    else
		    {
			    _include.ForEach(p => _includePatterns.Add(BuildPattern(p)));
		    }
			_excludePatterns = new List<Regex>();
		    _exclude.ForEach(p => _excludePatterns.Add(BuildPattern(p)));
	    }

		/// <summary>
		/// Converts a wildcard to a regex pattern
		/// </summary>
		public static Regex BuildPattern(string pattern)
		{
			return new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		}
	}
}

