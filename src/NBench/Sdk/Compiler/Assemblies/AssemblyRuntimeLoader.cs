// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NBench.Sdk.Compiler.Assemblies;
using NBench.Reporting;

#if  CORECLR
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
#endif

namespace NBench.Sdk.Compiler
{
    /// <summary>
    /// Utility class for loading assemblies with benchmarks at runtime
    /// </summary>
    internal static class AssemblyRuntimeLoader
    {
        /// <summary>
        /// Loads an assembly into the current AppDomain
        /// </summary>
        /// <param name="assemblyPath">The path to an assembly</param>
        /// <param name="trace">Optional. Benchmark tracing system.</param>
        /// <returns>An <see cref="IAssemblyLoader"/> with a reference to the <see cref="Assembly"/> at the specified location.</returns>
        public static IAssemblyLoader LoadAssembly(string assemblyPath, IBenchmarkOutput trace = null)
        {
            trace = trace ?? NoOpBenchmarkOutput.Instance;
#if CORECLR
            return new NetCoreAssemblyRuntimeLoader(assemblyPath, trace);
#else
            return new NetFrameworkAssemblyRuntimeLoader(assemblyPath, trace);
#endif
        }

        /// <summary>
        /// Loads an assembly into the current AppDomain
        /// </summary>
        /// <param name="assembly">An already-loaded assembly.</param>
        /// <param name="trace">Optional. Benchmark tracing system.</param>
        /// <returns>An <see cref="IAssemblyLoader"/> with a reference to the <see cref="Assembly"/> at the specified location.</returns>
        public static IAssemblyLoader WrapAssembly(Assembly assembly, IBenchmarkOutput trace = null)
        {
            trace = trace ?? NoOpBenchmarkOutput.Instance;
#if CORECLR
            return new NetCoreAssemblyRuntimeLoader(assembly, trace);
#else
            return new NetFrameworkAssemblyRuntimeLoader(assembly, trace);
#endif
        }
    }
}

