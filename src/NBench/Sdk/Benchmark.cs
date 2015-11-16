// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NBench.Metrics;
using NBench.Reporting;
using NBench.Util;

namespace NBench.Sdk
{
    /// <summary>
    ///     Executor class for running a single <see cref="PerformanceBenchmarkAttribute" />
    ///     Exposes the <see cref="BenchmarkContext" />, which allows developers to register custom
    ///     metrics and counters for the use of their personal benchmarks.
    /// </summary>
    public class Benchmark
    {
        /// <summary>
        ///     only use this for longer-running specs or <see cref="RunType.Throughput" />, where
        ///     metrics collection occurs on a separate thread.
        /// </summary>
        private readonly ManualResetEventSlim _changeRuns = new ManualResetEventSlim();

        protected readonly BenchmarkBuilder Builder;

        /// <summary>
        ///     Stopwatch used by the <see cref="Benchmark" />. Exposed only for testing purposes.
        /// </summary>
        public readonly Stopwatch StopWatch = new Stopwatch();

        private volatile BenchmarkRun _currentRun;

        /// <summary>
        ///     Indicates if we're in warm-up mode or not
        /// </summary>
        private bool _isWarmup = true;

        private int _pendingIterations;
        protected WarmupData WarmupData = WarmupData.Empty;

        public Benchmark(BenchmarkSettings settings, IBenchmarkInvoker invoker, IBenchmarkOutput writer)
        {
            Settings = settings;
            _pendingIterations = Settings.NumberOfIterations;
            Invoker = invoker;
            Output = writer;
            CompletedRuns = new Queue<BenchmarkRunReport>(Settings.NumberOfIterations);
            Builder = new BenchmarkBuilder(Settings);
        }

        public RunType RunMode => Settings.RunMode;
        public Queue<BenchmarkRunReport> CompletedRuns { get; }
        public BenchmarkSettings Settings { get; }
        public bool ShutdownCalled { get; private set; }
        protected IBenchmarkInvoker Invoker { get; }
        protected IBenchmarkOutput Output { get; }

        /// <summary>
        ///     Warmup phase
        /// </summary>
        private void WarmUp()
        {
            RunSingleBenchmark();

            // number of samples collected during the warmup.
            // serves as a rough estimate so we can pre-allocate all of our measures
            // in advance without having to allocate additional ones during the tests.
            var sampleCount = _currentRun.Measures[0].RawValues.Count;

            // elapsed time in Nanoseconds
            var runTime = StopWatch.Elapsed;

            WarmupData = new WarmupData(runTime, sampleCount);
        }

        /// <summary>
        ///     Pre-allocate all of the objects we're going to need for this benchmark
        /// </summary>
        private void Allocate()
        {
            _currentRun = Builder.NewRun(WarmupData);
        }

        /// <summary>
        /// Complete the current run
        /// </summary>
        private void Complete()
        {
            _currentRun.Dispose();

            var report = _currentRun.ToReport(StopWatch.Elapsed);
            Output.WriteRun(report, _isWarmup);

            // Change runs, but not on warmup
            if (!_isWarmup)
            {
                // Decrease the number of pending iterations
                _pendingIterations--;

                
                CompletedRuns.Enqueue(report);
            }
        }

        public void Run()
        {
            WarmUp();
            // disable further warmups
            _isWarmup = false;

            while (_pendingIterations > 0 && !ShutdownCalled)
            {
                RunSingleBenchmark();
            }
        }

        public void Shutdown()
        {
            ShutdownCalled = true;
            _changeRuns.Set();
        }

        public void RunSingleBenchmark()
        {
            Allocate(); // allocate all collectors needed
            // reset the stopwatch before each run
            StopWatch.Reset();
            PreRun();
            RunBenchmark();
            PostRun();
            Complete(); // release previous collectors
        }

        protected void PreRun()
        {
            // Invoke user-defined setup method, if any
            Invoker.InvokePerfSetup(_currentRun.Context);

            PrepareForRun();
        }


        /// <summary>
        /// Performs any final GC needed before we start a test run
        /// </summary>
        public static void PrepareForRun()
        {
            // Force a full garbage collection
            GC.Collect();

            // Wait for pending finalizers
            GC.WaitForPendingFinalizers();

            // One more time for good measure
            GC.Collect();
        }

        protected void RunBenchmark()
        {
            switch (RunMode)
            {
                case RunType.Iterations:
                    if (WarmupData.ElapsedTime <= BenchmarkConstants.SamplingPrecision)
                        RunIterationBenchmark();
                    else
                        RunThroughputBenchmark(true);
                    break;
                case RunType.Throughput:
                default:
                    RunThroughputBenchmark();
                    break;
            }
        }

        /// <summary>
        ///     NOTE: We don't reset the <see cref="Stopwatch" /> on purpose here, so we can
        ///     collect the value of it and use it for auto-tuning and reporting.
        ///     It'll be started at the beginning of the next run.
        /// </summary>
        protected void PostRun()
        {
            // Invoke user-defined cleanup method, if any
            Invoker.InvokePerfCleanup(_currentRun.Context);

            // Reset the ManualResetEvent
            _changeRuns.Reset();
        }

        /// <summary>
        /// Compiles all of the completed <see cref="BenchmarkRun"/>s into reports
        /// </summary>
        /// <returns>The full set of aggregate results</returns>
        public BenchmarkResults CompileResults()
        {
            return new BenchmarkResults(Invoker.BenchmarkName, Settings, CompletedRuns.ToList());
        }

        public BenchmarkFinalResults AssertResults(BenchmarkResults result)
        {
            var assertionResults = AssertionRunner.RunAssertions(Settings, result);
            return new BenchmarkFinalResults(result, assertionResults);
        }

        /// <summary>
        /// Complete the benchmark
        /// </summary>
        public void Finish()
        {
            var results = CompileResults();
            var finalResults = AssertResults(results);
            AllAssertsPassed = finalResults.AssertionResults.Any(x => !x.Passed);
            Output.WriteBenchmark(finalResults);
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="Finish"/> was called and all assertions passed,
        /// or if it was never called.
        /// </summary>
        public bool AllAssertsPassed { get; private set; }

        /// <summary>
        /// The name of this benchmark
        /// </summary>
        public string BenchmarkName => Invoker.BenchmarkName;

        #region RunModes

        private void RunIterationBenchmark()
        {
            _currentRun.Sample(StopWatch.Elapsed);
            StopWatch.Start();
            Invoker.InvokeRun(_currentRun.Context);
            StopWatch.Stop();

            //TODO: fixme
            _currentRun.Sample(StopWatch.Elapsed + TimeSpan.FromTicks(1L)); //add a tick just to ensure no collision
        }

        /// <summary>
        ///     Runs long-running benchmark and performs data collection in the background on a separate thread.
        ///     Occurs when <see cref="RunType.Throughput" /> is enabled or the <see cref="NBench.Sdk.WarmupData.ElapsedTime" />
        ///     is greater than <see cref="BenchmarkConstants.SamplingPrecision" />.
        /// </summary>
        /// <param name="isActuallyIteration">
        ///     When set to <c>true</c>, means that we run until the benchmark is finished and
        ///     only treat <see cref="BenchmarkSettings.RunTime" /> as a timeout rather than a duration. This
        ///     is designed for long-running benchmarks. 
        /// 
        ///     Default is <c>false</c>.
        /// </param>
        private void RunThroughputBenchmark(bool isActuallyIteration = false)
        {
            var runTime = Settings.RunTime;
            var currentContext = _currentRun.Context;
            var currentRun = _currentRun;

            // TODO: timeout mechanism for really long running runs
            // NOTE: will probably require a third "monitoring" thread

            // Foreground thread for running the benchmark
            var runThread = new Thread(_ =>
            {
                if (isActuallyIteration)
                    Invoker.InvokeRun(currentContext);
                else // don't loop if we're in Iteration mode
                {
                    while (StopWatch.Elapsed < runTime && !ShutdownCalled)
                    {
                        Invoker.InvokeRun(currentContext);
                    }
                }
               
                _changeRuns.Set();
            })
            { IsBackground = false };

            // Background thread for collecting samples on a fixed interval
            var sampleThread = new Thread(_ =>
            {
                while (true)
                {
                    currentRun.Sample(StopWatch.Elapsed);
                    Thread.Sleep(BenchmarkConstants.SamplingPrecision);
                    if (_changeRuns.IsSet)
                        return;
                }
            })
            { IsBackground = true };

            // GC anything that got created prior during the thread setup
            GC.Collect();

            // Take the first sample
            _currentRun.Sample(StopWatch.Elapsed);

            // Start!
            StopWatch.Start();
            runThread.Start();
            sampleThread.Start();

            //Wait
            _changeRuns.Wait();

            //Stop
            StopWatch.Stop();

            //Final collection
            _currentRun.Sample(StopWatch.Elapsed);

            // Release collectors
            _currentRun.Dispose();
        }

        #endregion
    }
}

