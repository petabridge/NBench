// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
            var assembly =  AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);        
            var deps = DependencyContext.Load(assembly).CompileLibraries
                .Where(d => d.Name.Contains(assembly.GetName().Name.ToLower()))
                .ToList();
            if (deps.Count > 0)
            {
                foreach (var dep in deps)
                {
                    foreach (var depDependency in dep.Dependencies)
                    {
                        try
                        {
                            AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetDirectoryName(assemblyPath)
                                                                             + Path.DirectorySeparatorChar
                                                                             + depDependency.Name + ".dll");
                        }
                        catch (System.IO.FileLoadException e)
                        {
                            // TODO: swallowed temporarily
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            // TODO: swallowed temporarily
                        }
                    }
                }
            }
            return assembly;
#else
            var targetAssembly = Assembly.LoadFrom(assemblyPath);
            return targetAssembly;
#endif
        }
    }
}

