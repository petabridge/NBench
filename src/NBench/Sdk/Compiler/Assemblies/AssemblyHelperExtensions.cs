using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NBench.Sdk.Compiler.Assemblies
{
    /// <summary>
    /// INTERNAL API.
    ///
    /// Used to make it easier to work with .NET Core and .NET Framework
    /// <see cref="Type"/> and <see cref="Assembly"/> instances.
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
