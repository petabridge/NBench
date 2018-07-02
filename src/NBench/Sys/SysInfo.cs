// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NBench.Sys
{
    /// <summary>
    /// Class that contains some basic information about the runtime environment
    /// </summary>
    public class SysInfo
    {
        private SysInfo(string os, string clrVersion, int processorCount, int workerThreads, int ioThreads, int maxGcGeneration, bool isMono)
        {
            
            OS = os;
            ProcessorCount = processorCount;
            WorkerThreads = workerThreads;
#if !CORECLR
            IOThreads = ioThreads;
#endif
            IsMono = isMono;
            ClrVersion = clrVersion;
            MaxGcGeneration = maxGcGeneration;
#if CORECLR
            NBenchAssemblyVersion = this.GetType().AssemblyQualifiedName;
#else
            NBenchAssemblyVersion = this.GetType().Assembly.FullName;
#endif
        }

        public string NBenchAssemblyVersion { get; private set; }

        /// <summary>
        /// Current version of the OS
        /// </summary>
        public string OS { get; private set; }

        /// <summary>
        /// Current version of the CLR
        /// </summary>
        public string ClrVersion { get; private set; }

        /// <summary>
        /// Number of logical processors
        /// </summary>
        public int ProcessorCount { get; private set; }

        /// <summary>
        /// Number of worker threads in <see cref="ThreadPool"/>
        /// </summary>
        public int WorkerThreads { get; private set; }

#if !CORECLR
        /// <summary>
        /// Number of I/O completion port threads in <see cref="ThreadPool"/>
        /// </summary>
        public int IOThreads { get; private set; }
#endif

        /// <summary>
        /// Maximum number of GC generations
        /// </summary>
        /// <remarks>
        /// If this system allows Gen 0, Gen 1, an Gen 2 GC then this value will be 2.
        /// If this system allows Gen 0, Gen 1, Gen 2, and Gen 3 then this value will be 3.
        /// </remarks>
        public int MaxGcGeneration { get; private set; }

        public bool IsMono { get; private set; }

        /// <summary>
        /// Singleton instance of <see cref="SysInfo"/>
        /// </summary>
        public readonly static SysInfo Instance = GetInstance();
   
        private static SysInfo GetInstance()
        {
            int maxGcGeneration = GC.MaxGeneration;
            bool isMono = Type.GetType("Mono.Runtime") != null;
            int workerThreads = 0;

#if !CORECLR
            int completionPortThreads = 0;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            return new SysInfo(Environment.OSVersion.ToString(), Environment.Version.ToString(), Environment.ProcessorCount, workerThreads, Environment.ProcessorCount, GC.MaxGeneration, isMono);
#else
            
            return new SysInfo(RuntimeInformation.OSDescription, RuntimeInformation.FrameworkDescription, Environment.ProcessorCount, workerThreads, Environment.ProcessorCount, maxGcGeneration, isMono);
#endif
        }
    }
}

