﻿#if CORECLR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using NBench.Reporting;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace NBench.Sdk.Compiler.Assemblies
{
    /// <inheritdoc />
    ///  <summary>
    ///  INTERNAL API.
    /// 
    ///  Used to run .NET Core assemblies
    ///  </summary>
    internal sealed class NetCoreAssemblyRuntimeLoader : IAssemblyLoader

    {
        private readonly ICompilationAssemblyResolver _resolver;
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;
        private readonly Lazy<Assembly[]> _referencedAssemblies;

        public NetCoreAssemblyRuntimeLoader(Assembly assembly, IBenchmarkOutput trace)
        {
            Assembly = assembly;

            _dependencyContext = DependencyContext.Load(Assembly);
            _loadContext = AssemblyLoadContext.GetLoadContext(Assembly);
            _resolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]{
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(Assembly.CodeBase)),
                new ReferenceAssemblyPathResolver(),
                new PackageCacheCompilationAssemblyResolver(),
                new PackageCompilationAssemblyResolver()});

            _loadContext.Resolving += LoadContextOnResolving;
            _referencedAssemblies = new Lazy<Assembly[]>(LoadReferencedAssemblies);
        }

        public NetCoreAssemblyRuntimeLoader(string path, IBenchmarkOutput trace)
        {
            if (!File.Exists(path))
            {
                trace.Error($"[NetCoreAssemblyRuntimeLoader] Unable to find requested assembly [{path}]");
                return;
            }

            Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            if (Assembly == null)
            {
                trace.Error($"[NetCoreAssemblyRuntimeLoader] Found assembly [{path}], but was unable to load it.");
                return;
            }
            _dependencyContext = DependencyContext.Load(Assembly);
            _loadContext = AssemblyLoadContext.GetLoadContext(Assembly);
            _resolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]{
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                new ReferenceAssemblyPathResolver(),
                new PackageCacheCompilationAssemblyResolver(),
                new PackageCompilationAssemblyResolver()});

            _loadContext.Resolving += LoadContextOnResolving;
            _referencedAssemblies = new Lazy<Assembly[]>(LoadReferencedAssemblies);
        }

        private Assembly[] LoadReferencedAssemblies()
        {
            var assemblies = new List<Assembly>();
            foreach (var assemblyName in Assembly.GetReferencedAssemblies())
            {
                try
                {
                    assemblies.Add(_loadContext.LoadFromAssemblyName(assemblyName));
                }
                catch
                {
                    // exception occurred, but we don't care
                }
            }

            return assemblies.ToArray();
        }

        private Assembly LoadContextOnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            bool NamesMatch(RuntimeLibrary runtime)
            {
                return string.Equals(runtime.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase);
            }

            var runtimeLibrary = _dependencyContext.RuntimeLibraries.FirstOrDefault(x => NamesMatch(x));
            if (runtimeLibrary != null)
            {
                var wrapper = new CompilationLibrary(runtimeLibrary.Type, runtimeLibrary.Name, 
                    runtimeLibrary.Version, runtimeLibrary.Hash, 
                    runtimeLibrary.RuntimeAssemblyGroups.SelectMany(a => a.AssetPaths), 
                    runtimeLibrary.Dependencies, runtimeLibrary.Serviceable);

                var loadedAssemblies = new List<string>();
                if (_resolver.TryResolveAssemblyPaths(wrapper, loadedAssemblies))
                {
                    return _loadContext.LoadFromAssemblyPath(loadedAssemblies[0]);
                }
            }

            return null;
        }

        public Assembly Assembly { get; }
        public Assembly[] ReferencedAssemblies => _referencedAssemblies.Value;

        public IEnumerable<Assembly> DependentAssemblies()
        {
            return Assembly.GetReferencedAssemblies().Select(x => _loadContext.LoadFromAssemblyName(x));
        }

        public void Dispose()
        {
            // clean up the event so we don't leak
            _loadContext.Resolving -= LoadContextOnResolving;
        }
    }
}

#endif