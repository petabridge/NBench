// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

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
        /// Assemblies that <see cref="Assembly"/> depends upon
        /// and its dependencies.
        /// </summary>
        Assembly[] AssemblyAndDependencies { get; }
    }
}

