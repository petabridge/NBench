﻿// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#if  CORECLR
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
#endif

namespace NBench.Sdk.Compiler
{
    // TODO: See issue https://github.com/petabridge/NBench/issues/3
    /// <summary>
    /// Utility class for loading assemblies with benchmarks at runtime
    /// </summary>
    public static class AssemblyRuntimeLoader
    {
        /// <summary>
        /// Gets the full path of an assembly
        /// </summary>
        /// <param name="assemblyPath">The short / relative path to an assembly</param>
        /// <returns>the full path of an assembly</returns>
        public static string GetFullAssemblyPath(string assemblyPath)
        {
            return Path.GetFullPath(assemblyPath);
        }

        /// <summary>
        /// Verifies if we can actually find an assembly at the specified location
        /// </summary>
        /// <param name="assemblyPath">The path to an assembly</param>
        /// <returns>true if we were able to find the assembly; false otherwise.</returns>
        public static bool CanFindAssembly(string assemblyPath)
        {
            //Can we determine that an assembly exists in this location?
            if (File.Exists(assemblyPath))
                return true;
            return false;
        }

        /// <summary>
        /// Loads an assembly into the current AppDomain
        /// </summary>
        /// <param name="assemblyPath">The path to an assembly</param>
        /// <returns>The assembly at the specified location</returns>
        public static Assembly LoadAssembly(string assemblyPath)
        {
#if CORECLR
            AssemblyLoadContext.Default.Resolving += (assemblyLoadContext, assemblyName) => DefaultOnResolving(assemblyLoadContext, assemblyName, assemblyPath);
            var targetAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            DependencyContext.Load(targetAssembly)
                .CompileLibraries
                .Where(dep => dep.Name.ToLower()
                    .Contains(targetAssembly.FullName.Split(new[] { ',' })[0].ToLower()))
                .Select(dependency => AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(dependency.Name)));
            return targetAssembly;
#else
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += ((sender, e) => ResolveAssembly(sender, e, assemblyPath));
            var targetAssembly = Assembly.LoadFrom(assemblyPath);
            return targetAssembly;
#endif
        }

#if !CORECLR
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs e, string assemblyPath)
        {
            string dllName = e.Name.Split(new[] { ',' })[0] + ".dll";
            return Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(assemblyPath), dllName));
        }
#else
        private static Assembly DefaultOnResolving(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName, string assemblyPath)
        {
            string dllName = assemblyName.Name.Split(new[] { ',' })[0] + ".dll";
            return assemblyLoadContext.LoadFromAssemblyPath(Path.Combine(Path.GetDirectoryName(assemblyPath), dllName));
        }
#endif
    }
}

