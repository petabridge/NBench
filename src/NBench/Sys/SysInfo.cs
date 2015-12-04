// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;

namespace NBench.Sys
{
    /// <summary>
    /// Class that contains some basic information about the runtime environment
    /// </summary>
    public class SysInfo
    {
        private SysInfo(OperatingSystem os, Version clrVersion, int processorCount, int workerThreads, int ioThreads, int maxGcGeneration, bool isMono)
        {
            OS = os;
            ProcessorCount = processorCount;
            WorkerThreads = workerThreads;
            IOThreads = ioThreads;
            IsMono = isMono;
            ClrVersion = clrVersion;
            MaxGcGeneration = maxGcGeneration;
            NBenchAssemblyVersion = this.GetType().Assembly.FullName;
        }

        public string NBenchAssemblyVersion { get; private set; }

        /// <summary>
        /// Current version of the OS
        /// </summary>
        public OperatingSystem OS { get; private set; }

        /// <summary>
        /// Current version of the CLR
        /// </summary>
        public Version ClrVersion { get; private set; }

        /// <summary>
        /// Number of logical processors
        /// </summary>
        public int ProcessorCount { get; private set; }

        /// <summary>
        /// Number of worker threads in <see cref="ThreadPool"/>
        /// </summary>
        public int WorkerThreads { get; private set; }

        /// <summary>
        /// Number of I/O completion port threads in <see cref="ThreadPool"/>
        /// </summary>
        public int IOThreads { get; private set; }

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
            OperatingSystem os = Environment.OSVersion;
            int processorCount = Environment.ProcessorCount;
            Version clrVersion = Environment.Version;
            int maxGcGeneration = GC.MaxGeneration;
            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
            bool isMono = Type.GetType("Mono.Runtime") != null;
            
            return new SysInfo(os, clrVersion, processorCount, workerThreads, processorCount, maxGcGeneration, isMono);
        }
    }
}

