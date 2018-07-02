// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

#if !CORECLR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NBench.Reporting;

namespace NBench.Sdk.Compiler.Assemblies
{
    /// <inheritdoc />
    ///  <summary>
    ///  INTERNAL API.
    ///  Used to run .NET Framework assemblies.
    ///  </summary>
    internal sealed class NetFrameworkAssemblyRuntimeLoader : IAssemblyLoader
    {
        private readonly string _binaryDirectory;
        private readonly Lazy<Assembly[]> _referencedAssemblies;
        private readonly IBenchmarkOutput _trace;

        public NetFrameworkAssemblyRuntimeLoader(Assembly assembly, IBenchmarkOutput trace)
        {
            _trace = trace;
            Assembly = assembly;
            _binaryDirectory = Path.GetDirectoryName(Assembly.CodeBase);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            _referencedAssemblies = new Lazy<Assembly[]>(LoadReferencedAssemblies);
        }

        public NetFrameworkAssemblyRuntimeLoader(string path, IBenchmarkOutput trace)
        {
            _trace = trace;
            if (!File.Exists(path))
            {
                trace.Error($"[NetFrameworkAssemblyRuntimeLoader] Unable to find requested assembly [{path}]");
                return;
            }

            Assembly = System.Reflection.Assembly.LoadFile(path);

            if (Assembly == null)
            {
                trace.Error($"[NetFrameworkAssemblyRuntimeLoader] Found assembly [{path}], but was unable to load it.");
                return;
            }

            _binaryDirectory = Path.GetDirectoryName(path);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            _referencedAssemblies = new Lazy<Assembly[]>(LoadReferencedAssemblies);
        }

        private Assembly[] LoadReferencedAssemblies()
        {
            var assemblies = new List<Assembly>(){ Assembly };
#if DEBUG
            _trace.WriteLine($"[NetFrameworkAssemblyRuntimeLoader][LoadReferencedAssemblies] Loading references for [{Assembly}]");
#endif
            foreach (var assemblyName in Assembly.GetReferencedAssemblies())
            {
                try
                {
#if DEBUG
                    _trace.WriteLine($"[NetFrameworkAssemblyRuntimeLoader][LoadReferencedAssemblies] Attempting to load [{assemblyName}]");
#endif
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch(Exception ex)
                {
                    // exception occurred, but we don't care
#if DEBUG
                    _trace.Error(ex, $"[NetCoreAssemblyRuntimeLoader][LoadReferencedAssemblies] Failed to load [{assemblyName}]");
#endif
                }
            }

            return assemblies.ToArray();
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return Assembly.LoadFile(Path.Combine(_binaryDirectory, args.Name));
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
        }

        public Assembly Assembly { get; }
        public Assembly[] AssemblyAndDependencies => _referencedAssemblies.Value;
    }
}

#endif

