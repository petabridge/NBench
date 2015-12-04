// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;

namespace NBench.Sdk
{
    /// <summary>
    ///     A <see cref="IBenchmarkInvoker" /> implementation that works on anonymous methods and delegates.
    /// </summary>
    public class ActionBenchmarkInvoker : IBenchmarkInvoker
    {
        /// <summary>
        ///     Default no-op action.
        /// </summary>
        public static readonly Action<BenchmarkContext> NoOp = context => { };

        private readonly Action<BenchmarkContext> _cleanupAction;
        private readonly Action<BenchmarkContext> _runAction;
        private readonly Action<BenchmarkContext> _setupAction;
        private Action<BenchmarkContext> _actualRunAction;

        public ActionBenchmarkInvoker(string benchmarkName, Action<BenchmarkContext> runAction)
            : this(benchmarkName, NoOp, runAction, NoOp)
        {
        }

        public ActionBenchmarkInvoker(string benchmarkName,
            Action<BenchmarkContext> setupAction,
            Action<BenchmarkContext> runAction,
            Action<BenchmarkContext> cleanupAction)
        {
            BenchmarkName = benchmarkName;
            _setupAction = setupAction;
            _actualRunAction = _runAction = runAction;
            _cleanupAction = cleanupAction;
        }

        public string BenchmarkName { get; }

        public void InvokePerfSetup(BenchmarkContext context)
        {
            _setupAction(context);
        }

        public void InvokePerfSetup(long runCount, BenchmarkContext context)
        {
            _actualRunAction = benchmarkContext =>
            {
                for (var i = runCount; i != 0;)
                {
                    _runAction(context);
                    --i;
                }
            };
            InvokePerfSetup(context);
        }

        public void InvokeRun(BenchmarkContext context)
        {
           _actualRunAction(context);
        }

        public void InvokePerfCleanup(BenchmarkContext context)
        {
            _cleanupAction(context);
        }
    }
}

