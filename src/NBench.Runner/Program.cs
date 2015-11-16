// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using NBench.Reporting;
using NBench.Reporting.Targets;
using NBench.Sdk;
using NBench.Sdk.Compiler;

namespace NBench.Runner
{
    class Program
    {
        protected static IBenchmarkOutput Output;
        protected static IDiscovery Discovery;
        

        /// <summary>
        /// NBench Runner takes the following <see cref="args"/>
        /// 
        /// C:\> NBench.Runner.exe [assembly name]
        /// 
        /// </summary>
        /// <param name="args">The commandline arguments</param>
        static int Main(string[] args)
        {
            Output = new CompositeBenchmarkOutput(new ConsoleBenchmarkOutput());
            Discovery = new ReflectionDiscovery(Output);
            string assemblyPath = Path.GetFullPath(args[0]);

            // TODO: See issue https://github.com/petabridge/NBench/issues/3
            var assembly = AssemblyRuntimeLoader.LoadAssembly(assemblyPath);

            var benchmarks = Discovery.FindBenchmarks(assembly);
            bool anyAssertFailures = false;
            foreach (var benchmark in benchmarks)
            {
                Output.WriteStartingBenchmark(benchmark.BenchmarkName);
                benchmark.Run();
                benchmark.Finish();

                // if one assert fails, all fail
                anyAssertFailures = anyAssertFailures || !benchmark.AllAssertsPassed; 
            }

            Console.ReadLine();
            return anyAssertFailures ? -1 : 0;
        }
    }
}

