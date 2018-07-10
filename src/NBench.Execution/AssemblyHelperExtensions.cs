// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace NBench
{
    /// <summary>
    /// INTERNAL API.
    ///
    /// Used to make it easier to work with .NET Core and .NET Framework
    /// <see cref="System.Type"/> and <see cref="System.Reflection.Assembly"/> instances.
    /// </summary>
    internal static class AssemblyHelperExtensions
    {
        public static Assembly GetAssembly(this Type t)
        {
#if CORECLR
            return t.GetTypeInfo().Assembly; // extra step for .NET Core
#else
            return t.Assembly;
#endif
        }
    }
}

