// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// Inspired by https://github.com/xunit/xunit/blob/master/src/dotnet-xunit/Program.cs

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using static NBench.MsBuildHelpers;

namespace NBench.Runner.DotNetCli
{
    class Program
    {
        private static readonly Version Version452 = new Version("4.5.2");
        private string _configuration;
        private string _fxVersion;
        private bool _internalDiagnostics;
        private bool _noBuild;
        private string _thisAssemblyPath;
        private string _buildStdProps;

        /// <summary>
        /// NBench Runner takes the following <see cref="args"/>
        /// 
        /// C:\> dotnet nbench [assembly name] [output-directory={dir-path}] [configuration={file-path}] [include=MyTest*.Perf*,Other*Spec] [exclude=*Long*] [concurrent={true|false}] [trace={true|false}] [teamcity={true|false}]
        /// 
        /// </summary>
        /// <param name="args">The commandline arguments</param>
        public static int Main(string[] args)
        {
            return new Program().Execute(args);
        }

        int Execute(string[] args)
        {
            // Let Ctrl+C pass down into the child processes, ignoring it here
            Console.CancelKeyPress += (sender, e) => e.Cancel = true;

            try
            {
                if (args.Length == 1 && args[0] == "--help")
                {
                    CommandLine.ShowHelp();
                    return 0;
                }

                if (CommandLine.HasProperty(CommandLine.DiagnosticsKey))
                    _internalDiagnostics = true;

                // The extra versions are unadvertised compatibility flags to match 'dotnet' command line switches
                var requestedTargetFramework = (CommandLine.GetProperty("-framework")
                                                   ?? CommandLine.GetProperty("--framework")
                                                   ?? CommandLine.GetProperty("-f"))?.SingleOrDefault();

                _configuration = (CommandLine.GetProperty("-configuration")
                                ?? CommandLine.GetProperty("--configuration")
                                ?? CommandLine.GetProperty("-c"))?.SingleOrDefault()
                                ?? "Release";

                _fxVersion = (CommandLine.GetProperty("-fxversion")
                            ?? CommandLine.GetProperty("--fx-version"))?.SingleOrDefault();

                _noBuild = (CommandLine.HasProperty("-nobuild")
                          || CommandLine.HasProperty("--no-build"));

                var testProjects = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.*proj")
                    .Where(x => !x.EndsWith(".xproj")) // skip the beta .NET Core format
                    .ToList();

                if (testProjects.Count == 0)
                {
                    WriteLineError("Could not find any project (*.*proj) file in the current directory.");
                    return 3;
                }

                if (testProjects.Count > 1)
                {
                    WriteLineError($"Multiple project files were found; only a single project file is supported. Found: {string.Join(", ", testProjects.Select(x => Path.GetFileName(x)))}");
                    return 3;
                }

                _thisAssemblyPath = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);
                _buildStdProps = $"\"/p:_NBench_ImportTargetsFile={Path.Combine(_thisAssemblyPath, "import.targets")}\" " +
                                $"/p:Configuration={_configuration}";

                var testProject = testProjects[0];

                var targetFrameworks = GetTargetFrameworks(testProject);
                if (targetFrameworks == null)
                {
                    WriteLineError($"Failed to detect supported frameworks for [{testProject}]. Please ensure that this is a valid MSBuild15 project.");
                    return 3;
                }

                if (requestedTargetFramework != null)
                {
                    if (!targetFrameworks.Contains(requestedTargetFramework, StringComparer.OrdinalIgnoreCase))
                    {
                        WriteLineError($"Unknown target framework '{requestedTargetFramework}'; available frameworks: {string.Join(", ", targetFrameworks.Select(f => $"'{f}'"))}");
                        return 3;
                    }

                    return RunTargetFramework(testProject, requestedTargetFramework);
                }

                var returnValue = 0;

                foreach (var targetFramework in targetFrameworks)
                {
                    var result = RunTargetFramework(testProject, targetFramework);
                    if (result < 0)
                        return result;

                    returnValue = Math.Max(result, returnValue);
                }

                return returnValue;

            }
            catch (Exception ex)
            {
                WriteLineError($"Error: {ex.Message}");
                WriteLineError($"Source {ex.Source}");
                WriteLineError($"StackTrace: {ex.StackTrace}");
                return 3;
            }
        }

        public int RunTargetFramework(string testProject, string targetFramework)
        {
            var tmpFile = Path.GetTempFileName();
            try
            {
                string target;

                if (_noBuild)
                {
                    target = "_NBench_GetTargetValues";
                    WriteLine($"Locating binaries for framework {targetFramework}...");
                }
                else
                {
                    target = "Build;_NBench_GetTargetValues";
                    WriteLine($"Building for framework {targetFramework}...");
                }

                var psi = GetMsBuildProcessStartInfo(testProject);
                psi.Arguments += $"/t:{target} \"/p:_NBenchInfoFile={tmpFile}\" \"/p:TargetFramework={targetFramework}\"";
                WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");

                var process = Process.Start(psi);
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    WriteLineError("Build failed!");
                    return process.ExitCode;
                }

                var lines = File.ReadAllLines(tmpFile);
                var outputPath = "";
                var assemblyName = "";
                var targetFileName = "";
                var targetFrameworkIdentifier = "";
                var targetFrameworkVersion = "";
                var runtimeFrameworkVersion = "";

                foreach (var line in lines)
                {
                    var idx = line.IndexOf(':');
                    if (idx <= 0) continue;
                    var name = line.Substring(0, idx)?.Trim().ToLowerInvariant();
                    var value = line.Substring(idx + 1)?.Trim();
                    if (name == "outputpath")
                        outputPath = value;
                    else if (name == "assemblyname")
                        assemblyName = value;
                    else if (name == "targetfilename")
                        targetFileName = value;
                    else if (name == "targetframeworkidentifier")
                        targetFrameworkIdentifier = value;
                    else if (name == "targetframeworkversion")
                        targetFrameworkVersion = value;
                    else if (name == "runtimeframeworkversion")
                        runtimeFrameworkVersion = value;
                }

                var version = string.IsNullOrWhiteSpace(targetFrameworkVersion) ? new Version("0.0.0.0") : new Version(targetFrameworkVersion.TrimStart('v'));

                if (targetFrameworkIdentifier == ".NETCoreApp")
                {
                    if (runtimeFrameworkVersion == "2.0")
                        runtimeFrameworkVersion = "2.0.0";

                    string netCoreAppVersion = $"netcoreapp{version.Major}.{version.Minor}";

                    var fxVersion = _fxVersion ?? runtimeFrameworkVersion;
                    WriteLine($"Running .NET Core {fxVersion} tests for framework {targetFramework}...");
                    var outputDirectory = SetFrameworkOutputDirectory(netCoreAppVersion, testProject);
                    return RunDotNetCoreProject(outputPath, assemblyName, targetFileName, fxVersion, netCoreAppVersion, outputDirectory);
                }
                if (targetFrameworkIdentifier == ".NETFramework" && version >= Version452)
                {
                    WriteLine($"Running desktop CLR tests for framework {targetFramework}...");
                    var outputDirectory = SetFrameworkOutputDirectory(targetFramework, testProject);
                    return RunDesktopProject(outputPath, targetFileName, outputDirectory);
                }

                WriteLineWarning($"Unsupported target framework '{targetFrameworkIdentifier} {version}' (only .NETCoreApp 1.x/2.x and .NETFramework 4.5.2+ are supported)");
                return 0;
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        private int RunDesktopProject(string outputPath, string targetFileName, string outputDirectory)
        {
            var runnerFolder = Path.GetFullPath(Path.Combine(_thisAssemblyPath, "..", "..", "tools", "net452"));

            // Debug hack to be able to run from the compilation folder
            if (!Directory.Exists(runnerFolder))
                runnerFolder = Path.GetFullPath(Path.Combine(_thisAssemblyPath, "..", "..", "..", "..", "NBench.Runner", "bin", "Debug", "net452", "win7-x64"));

            // Release hack to run during FAKE builds
            if (!Directory.Exists(runnerFolder))
                runnerFolder = Path.GetFullPath(Path.Combine(_thisAssemblyPath, "..", "..", "..", "..", "NBench.Runner", "bin", "Release", "net452", "win7-x64"));

            var executableName = "NBench.Runner.exe";

            var psi = new ProcessStartInfo
            {
                FileName = Path.Combine(runnerFolder, executableName),
                Arguments = $@"""{targetFileName}"" {CommandLine.FormatCapturedArguments(false)} {CommandLine.OutputKey}=""{outputDirectory}""",
                WorkingDirectory = Path.GetFullPath(outputPath)
            };

            WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");
            WriteLineDiagnostics($"  IN: {psi.WorkingDirectory}");

            var runTests = Process.Start(psi);
            runTests.WaitForExit();

            return runTests.ExitCode;
        }

        private int RunDotNetCoreProject(string outputPath, string assemblyName, string targetFileName, string fxVersion, string netCoreAppVersion, string outputDirectory)
        {
            var consoleFolder = Path.GetFullPath(Path.Combine(_thisAssemblyPath, "..", "..", "tools", netCoreAppVersion));

            // Debug hack to be able to run from the compilation folder
            if (!Directory.Exists(consoleFolder))
                consoleFolder = Path.GetFullPath(Path.Combine(_thisAssemblyPath, "..", "..", "..", "..", "NBench.Runner", "bin", "Debug", netCoreAppVersion));

            // Release hack to run during FAKE builds
            if (!Directory.Exists(consoleFolder))
                consoleFolder = Path.GetFullPath(Path.Combine(_thisAssemblyPath, "..", "..", "..", "..", "NBench.Runner", "bin", "Release", netCoreAppVersion));

            if (!Directory.Exists(consoleFolder))
            {
                WriteLineError($"Could not locate runner DLL for {netCoreAppVersion}; unsupported version of .NET Core");
                return 3;
            }

            var runner = Path.Combine(consoleFolder, "NBench.Runner.dll");
            var workingDirectory = Path.GetFullPath(outputPath);
            var targetFileNameWithoutExtension = Path.GetFileNameWithoutExtension(targetFileName);
            var depsFile = targetFileNameWithoutExtension + ".deps.json";
            var runtimeConfigJson = targetFileNameWithoutExtension + ".runtimeconfig.json";

            var args = $@"exec --fx-version {fxVersion} --depsfile ""{depsFile}"" ";

            if (File.Exists(Path.Combine(workingDirectory, runtimeConfigJson)))
                args += $@"--runtimeconfig ""{runtimeConfigJson}"" ";

            args += $@"""{runner}"" ""{targetFileName}"" {CommandLine.FormatCapturedArguments(false)}""{CommandLine.OutputKey}={outputDirectory}""";

            var psi = new ProcessStartInfo { FileName = DotNetMuxer.MuxerPath, Arguments = args, WorkingDirectory = workingDirectory };

            WriteLineDiagnostics($"EXEC: \"{psi.FileName}\" {psi.Arguments}");
            WriteLineDiagnostics($"  IN: {psi.WorkingDirectory}");

            var runTests = Process.Start(psi);
            runTests.WaitForExit();
            return runTests.ExitCode;
        }

        /// <summary>
        /// Creates an output directory for NBench output if one isn't already explicitly set by the user.
        /// </summary>
        /// <param name="targetFramework">The framework the user is using</param>
        /// <param name="testProject">The test project</param>
        /// <param name="createIfNotExists">Create the root output directory if it doesn't exist.</param>
        /// <returns></returns>
        string SetFrameworkOutputDirectory(string targetFramework, string testProject, bool createIfNotExists = true)
        {
            string outputDir;
            if (CommandLine.HasProperty("output-directory"))
                outputDir = CommandLine.GetSingle("output-directory");
            else
                outputDir = Path.Combine(new FileInfo(testProject).DirectoryName, "PerfResults");

            WriteLine($"OutputDir {outputDir}");

            if (createIfNotExists && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            try
            {
                return Path.Combine(outputDir, targetFramework);
            }
            catch
            {
                WriteLineError($"Ran into error while attempting to construct path {outputDir}/{targetFramework}");
                throw;
            }
        }

        ProcessStartInfo GetMsBuildProcessStartInfo(string testProject)
        {
            var args = $"\"{testProject}\" /nologo {_buildStdProps} ";

            return new ProcessStartInfo { FileName = DotNetMuxer.MuxerPath, Arguments = $"msbuild {args}" };
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteLineDiagnostics(string message)
        {
            if (_internalDiagnostics)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        public void WriteLineError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void WriteLineWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}

