// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NBench.Sdk
{
    /// <summary>
    /// A TestPackage contains one or more test files. It also holds settings how the tests should be loaded. 
    /// </summary>
#if !CORECLR
    [Serializable]
#endif
    public class TestPackage
#if !CORECLR
        : MarshalByRefObject
#endif
    {
        /// <summary>
        /// List of assemblies to be loaded and tested
        /// </summary>
        private List<string> _testfiles = new List<string>();

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
		/// Gets the file names of the assemblies containing tests
		/// </summary>
		public IEnumerable<string> Files
        {
            get { return _testfiles; }
        }

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
        /// Initializes a new test package with one test file.
        /// </summary>
        /// <param name="filePath">The path to a test file.</param>
        /// <param name="include">An optional include pattern</param>
        /// <param name="exclude">An optiona exclude pattern</param>
        /// <param name="concurrent">Enable benchmarks that use multiple threads. See <see cref="Concurrent"/> for more details.</param>
        public TestPackage(string filePath, IEnumerable<string> include = null, IEnumerable<string> exclude = null, bool concurrent = false)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			AddSingleFile(filePath);

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
        /// <param name="files">A list of test files.</param>
        /// <param name="include">Optional list of include patterns</param>
        /// <param name="exclude">Optional list of exclude patterns</param>
        /// <param name="concurrent">Enable benchmarks that use multiple threads. See <see cref="Concurrent"/> for more details.</param>
        public TestPackage(IEnumerable<string> files, IEnumerable<string> include = null, IEnumerable<string> exclude = null, bool concurrent = false)
        {
            var enumerable = files as string[] ?? files.ToArray();
            if (files == null || !enumerable.Any())
                throw new ArgumentException("Please provide at least one test file." ,nameof(files));

			// if only one file is given use same common logic
			if (enumerable.Count() == 1)
				AddSingleFile(enumerable.ElementAt(0));
			else
			{
				foreach (var file in enumerable)
					_testfiles.Add(Path.GetFullPath(file));
			}

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
        /// Validates the test package
        /// </summary>
        public void Validate()
        {
            // validate the files
            foreach(var file in _testfiles)
            {
                if (!File.Exists(file))
                    throw new FileNotFoundException($"Test file '{file}' could not be found!");
            }

            // validation configuration file
            if (!string.IsNullOrEmpty(ConfigurationFile))
            {
                if (!Path.IsPathRooted(ConfigurationFile))
                    ConfigurationFile = Path.Combine(GetBasePath(), ConfigurationFile);

                if (!File.Exists(ConfigurationFile))
                    throw new FileNotFoundException($"Configuration file '{ConfigurationFile}' could not be found!");
            }
        }

        /// <summary>
        /// Gets the base path for this test package
        /// </summary>
        /// <returns></returns>
        public string GetBasePath()
        {
            return Path.GetDirectoryName(_testfiles[0]);
        }

#if !CORECLR
        /// <summary>
        /// Control the lifetime policy for this instance
        /// </summary>
        public override object InitializeLifetimeService()
		{
			// Live forever
			return null;
		}
#endif

        /// <summary>
        /// Adds a single file to the package
        /// </summary>
        /// <param name="filePath">The path to a test file.</param>
        private void AddSingleFile(string filePath)
		{
			_testfiles.Add(Path.GetFullPath(filePath));

			// use config file of the given assembly if available
			var configFile = _testfiles[0] + ".config";

			if (File.Exists(configFile))
				ConfigurationFile = configFile;

			Name = Path.GetFileNameWithoutExtension(filePath);
		}

		/// <summary>
		/// Add a pattern to be excluded. We'll ignore nulls.
		/// </summary>
		/// <param name="exclude"></param>
		private void AddExclude(string exclude)
		{
			if (String.IsNullOrEmpty(exclude))
				return;
			
			_exclude.Add(exclude);
		}

		/// <summary>
		/// Add a pattern to be included. We'll ignore nulls.
		/// </summary>
		/// <param name="include"></param>
		private void AddInclude(string include)
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

