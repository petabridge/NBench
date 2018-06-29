#if NETFRAMEWORK

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
    internal sealed class NetFrameworkAssemblyLoader : IAssemblyLoader
    {
        private readonly string _binaryDirectory;
        private readonly Lazy<Assembly[]> _referencedAssemblies;

        public NetFrameworkAssemblyLoader(Assembly assembly, IBenchmarkOutput trace) {
            Assembly = assembly;
            _binaryDirectory = Path.GetDirectoryName(Assembly.CodeBase);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            _referencedAssemblies = new Lazy<Assembly[]>(LoadReferencedAssemblies);
        }

        public NetFrameworkAssemblyLoader(string path, IBenchmarkOutput trace)
        {
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
            foreach (var assemblyName in Assembly.GetReferencedAssemblies())
            {
                try
                {
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch
                {
                    // exception occurred, but we don't care
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