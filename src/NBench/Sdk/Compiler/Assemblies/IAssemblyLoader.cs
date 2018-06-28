using System;
using System.Reflection;

namespace NBench.Sdk.Compiler.Assemblies
{
    /// <inheritdoc />
    ///  <summary>
    ///  INTERNAL API.
    ///  Used to load and resolve assemblies as part of NBench test discovery and execution.
    ///  </summary>
    public interface IAssemblyLoader : IDisposable
    {
        /// <summary>
        /// The primary assembly we're loading.
        /// </summary>
        Assembly Assembly { get; }

        /// <summary>
        /// Assemblies that <see cref="Assembly"/> depends upon.
        /// </summary>
        Assembly[] ReferencedAssemblies { get; }
    }
}