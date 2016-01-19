using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBench.Sdk
{
    /// <summary>
    /// A TestPackage contains one or more test files. It also holds settings how the tests should be loaded. 
    /// </summary>
    [Serializable]
    public class TestPackage : MarshalByRefObject
    {
        private List<string> _testfiles = new List<string>();
        
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
		/// Gets the file names of the assemblies containing tests
		/// </summary>
		public IEnumerable<string> Files
        {
            get { return _testfiles; }
        }

        /// <summary>
        /// Initializes a new test package with one test file.
        /// </summary>
        /// <param name="filePath">The path to a test file.</param>
        public TestPackage(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");

			AddSingleFile(filePath);
		}		

		/// <summary>
		/// Initializes a new package with multiple test files.
		/// </summary>
		/// <param name="files">A list of test files.</param>
		public TestPackage(IEnumerable<string> files)
        {
            if (files == null || !files.Any())
                throw new ArgumentException("Please provide at least one test file." ,"files");

			// if only one file is given use same common logic
			if (files.Count() == 1)
				AddSingleFile(files.ElementAt(0));
			else
			{
				foreach (var file in files)
					_testfiles.Add(Path.GetFullPath(file));
			}
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
                    throw new FileNotFoundException(string.Format("Test file '{0}' could not be found!", file));
            }

            // validation configuration file
            if (!string.IsNullOrEmpty(ConfigurationFile))
            {
                if (!Path.IsPathRooted(ConfigurationFile))
                    ConfigurationFile = Path.Combine(GetBasePath(), ConfigurationFile);

                if (!File.Exists(ConfigurationFile))
                    throw new FileNotFoundException(string.Format("Configuration file '{0}' could not be found!", ConfigurationFile));
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

		/// <summary>
		/// Control the lifetime policy for this instance
		/// </summary>
		public override object InitializeLifetimeService()
		{
			// Live forever
			return null;
		}

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
	}	
}
