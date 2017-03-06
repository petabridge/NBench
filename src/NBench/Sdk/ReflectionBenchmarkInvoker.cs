// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using NBench.Sdk.Compiler;

namespace NBench.Sdk
{
    /// <summary>
    ///     <see cref="IBenchmarkInvoker" /> implementaton that uses reflection to invoke setup / run / cleanup methods
    ///     found on classes decorated with the appropriate <see cref="PerfBenchmarkAttribute" />s.
    /// </summary>
    public class ReflectionBenchmarkInvoker : IBenchmarkInvoker
    {
        private readonly BenchmarkClassMetadata _metadata;
        private Action<BenchmarkContext> _cleanupAction;
        private Action<BenchmarkContext> _runAction;
        private Action<BenchmarkContext> _setupAction;
        private object _testClassInstance;

        public ReflectionBenchmarkInvoker(BenchmarkClassMetadata metadata)
        {
            _metadata = metadata;
            BenchmarkName = $"{metadata.BenchmarkClass.FullName}+{metadata.Run.InvocationMethod.Name}";
        }

        public string BenchmarkName { get; }

        public void InvokePerfSetup(BenchmarkContext context)
        {
            _testClassInstance = Activator.CreateInstance(_metadata.BenchmarkClass);
            _cleanupAction = Compile(_metadata.Cleanup);
            _setupAction = Compile(_metadata.Setup);
            _runAction = Compile(_metadata.Run);

            _setupAction(context);
        }

        public void InvokePerfSetup(long runCount, BenchmarkContext context)
        {
            InvokePerfSetup(context);

            var previousRunAction = _runAction;

            _runAction = ctx =>
            {
                for (long i = runCount; i != 0;)
                {
                    previousRunAction(ctx);
                    --i;
                }
            };
        }

        public void InvokeRun(BenchmarkContext context)
        {
            _runAction(context);
        }

        public void InvokePerfCleanup(BenchmarkContext context)
        {
            // cleanup method
            _cleanupAction(context);

            // instance cleanup
            var disposable = _testClassInstance as IDisposable;
            disposable?.Dispose();

            _testClassInstance = null;
            _setupAction = null;
            _cleanupAction = null;
            _runAction = null;
        }

        internal static Action<BenchmarkContext> CreateDelegateWithContext(object target, MethodInfo invocationMethod)
        {
            var del =
                (Action<BenchmarkContext>)
                    invocationMethod.CreateDelegate(typeof(Action<BenchmarkContext>), target);
            return del;
        }

        internal static Action<BenchmarkContext> CreateDelegateWithoutContext(object target, MethodInfo invocationMethod)
        {
            var del =
                (Action)
                    invocationMethod.CreateDelegate(typeof(Action), target);

            Action<BenchmarkContext> wrappedDelegate = context => del();
            return wrappedDelegate;
        }

        private Action<BenchmarkContext> Compile(BenchmarkMethodMetadata metadata)
        {
            if (metadata.Skip)
                return ActionBenchmarkInvoker.NoOp;
            return metadata.TakesBenchmarkContext
                ? CreateDelegateWithContext(_testClassInstance, metadata.InvocationMethod)
                : CreateDelegateWithoutContext(_testClassInstance, metadata.InvocationMethod);
        }
    }
}