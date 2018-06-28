﻿#if NETFRAMEWORK

using System;
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
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            return System.Reflection.Assembly.LoadFile(Path.Combine(_binaryDirectory, args.Name));
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
        }

        public Assembly Assembly { get; }
    }
}

#endif