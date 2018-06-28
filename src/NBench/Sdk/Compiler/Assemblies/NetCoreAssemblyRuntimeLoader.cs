#if CORECLR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace NBench.Sdk.Compiler.Assemblies
{
    /// <summary>
    /// INTERNAL API.
    ///
    /// Used to run .NET Core assemblies
    /// </summary>
    internal sealed class NetCoreAssemblyRuntimeLoader : IDisposable

    {
        private readonly ICompilationAssemblyResolver _resolver;
        private readonly DependencyContext _dependencyContext;
        private readonly AssemblyLoadContext _loadContext;
        public NetCoreAssemblyRuntimeLoader(string path, IBenchmarkTrace trace)
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
            }
            _dependencyContext = DependencyContext.Load(Assembly);
            _loadContext = AssemblyLoadContext.GetLoadContext(Assembly);
            _resolver = new CompositeCompilationAssemblyResolver(new ICompilationAssemblyResolver[]{
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(path)),
                new ReferenceAssemblyPathResolver(),
                new PackageCacheCompilationAssemblyResolver(),
                new PackageCompilationAssemblyResolver()});

            _loadContext.Resolving += LoadContextOnResolving;
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

        public void Dispose()
        {
            // clean up the event so we don't leak
            _loadContext.Resolving -= LoadContextOnResolving;
        }
    }
}

#endif