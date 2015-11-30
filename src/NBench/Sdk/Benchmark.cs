// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using NBench.Metrics;
using NBench.Reporting;

namespace NBench.Sdk
{
    /// <summary>
    ///     Executor class for running a single <see cref="PerfBenchmarkAttribute" />
    ///     Exposes the <see cref="BenchmarkContext" />, which allows developers to register custom
    ///     metrics and counters for the use of their personal benchmarks.
    /// </summary>
    public class Benchmark
    {
        protected readonly BenchmarkBuilder Builder;

        /// <summary>
        ///     Stopwatch used by the <see cref="Benchmark" />. Exposed only for testing purposes.
        /// </summary>
        public readonly Stopwatch StopWatch = new Stopwatch();

        private BenchmarkRun _currentRun;

        /// <summary>
        ///     Indicates if we're in warm-up mode or not
        /// </summary>
        private bool _isWarmup = true;

        private int _pendingIterations;
        protected WarmupData WarmupData = WarmupData.PreWarmup;

        public Benchmark(BenchmarkSettings settings, IBenchmarkInvoker invoker, IBenchmarkOutput writer)
        {
            Settings = settings;
            _pendingIterations = Settings.NumberOfIterations;
            Invoker = invoker;
            Output = writer;
            CompletedRuns = new Queue<BenchmarkRunReport>(Settings.NumberOfIterations);
            Builder = new BenchmarkBuilder(Settings);
        }

        public RunMode RunMode => Settings.RunMode;
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
            var warmupStopWatch = new Stopwatch();
            var targetTime = Settings.RunTime;
            Contract.Assert(targetTime != TimeSpan.Zero);
            var runCount = 0L;

            /* Pre-Warmup */
            RunSingleBenchmark();

            // check to see if pre-warmup threw an exception
            var faulted = _currentRun.IsFaulted;

            if (faulted)
            {
                /*
                 * Normally we don't ever queue up the warmup into the final stats, but we do it
                 * in failure cases so we can capture the exception thrown during warmup into
                 * the final report we're going to deliver to the end-user.
                 */
                CompletedRuns.Enqueue(_currentRun.ToReport(TimeSpan.Zero));

                return;
            }

            /* Warmup */
            Allocate(); // allocate all collectors needed
            PreRun();
            warmupStopWatch.Start();
            while (warmupStopWatch.ElapsedTicks < targetTime.Ticks)
            {
                Invoker.InvokeRun(_currentRun.Context);
                runCount++;
            }
            warmupStopWatch.Stop();
            PostRun();

            // elapsed time
            var runTime = warmupStopWatch.ElapsedTicks;

            WarmupData = new WarmupData(runTime, runCount);
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

            while (_pendingIterations > 0 && !_currentRun.IsFaulted)
            {
                RunSingleBenchmark();
            }

            // Bailing out if the benchmark was faulted
            if (_currentRun.IsFaulted)
            {
                Output.Warning($"Error during previous run of {BenchmarkName}. Aborting run...");
            }
            
        }

        public void Shutdown()
        {
            ShutdownCalled = true;
        }

        private void RunSingleBenchmark()
        {
            Allocate(); // allocate all collectors needed
            // reset the stopwatch before each run
            StopWatch.Reset();
            PreRun();
            try
            {
                RunBenchmark();
            }
            catch (Exception ex)
            {
                HandleBenchmarkRunExecption(ex, $"Error occurred during ${BenchmarkName} RUN.");
            }
            PostRun();
            Complete(); // release previous collectors
        }

        protected void PreRun()
        {
            try
            {
                // Invoke user-defined setup method, if any
                Invoker.InvokePerfSetup(_currentRun.Context);

                PrepareForRun();
            }
            catch (Exception ex)
            {
                HandleBenchmarkRunExecption(ex, $"Error occurred during ${BenchmarkName} SETUP.");
            }
        }

        private void HandleBenchmarkRunExecption(Exception ex, string formatMsg)
        {
            var nbe = new NBenchException(formatMsg, ex);
            _currentRun.WithException(nbe);
            Output.Error(ex, nbe.Message);
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
                case RunMode.Iterations:
                    RunIterationBenchmark();
                    break;
                case RunMode.Throughput:
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
            try
            {
                // Invoke user-defined cleanup method, if any
                Invoker.InvokePerfCleanup(_currentRun.Context);
            }
            catch (Exception ex)
            {
                HandleBenchmarkRunExecption(ex, $"Error occurred during ${BenchmarkName} CLEANUP.");
            }
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
            _currentRun.Sample(StopWatch.ElapsedTicks);
            StopWatch.Start();
            Invoker.InvokeRun(_currentRun.Context);
            StopWatch.Stop();
            _currentRun.Sample(StopWatch.ElapsedTicks); //add a tick just to ensure no collision
        }

        /// <summary>
        ///     Runs long-running benchmark and performs data collection in the background on a separate thread.
        ///     Occurs when <see cref="NBench.RunMode.Throughput" /> is enabled or the <see cref="NBench.Sdk.WarmupData.ElapsedTime" />
        ///     is greater than <see cref="BenchmarkConstants.SamplingPrecisionTicks" />.
        /// </summary>
        private void RunThroughputBenchmark()
        {
            var currentContext = _currentRun.Context;


            var runCount = WarmupData.EstimatedRunsPerSecond;
            _currentRun.Sample(StopWatch.ElapsedTicks);

            // Start!
            StopWatch.Start();

            while (true)
            {
                Invoker.InvokeRun(currentContext);
                if (runCount-- == 0)
                    break;
            }

            //Stop
            StopWatch.Stop();

            //Final collection
            _currentRun.Sample(StopWatch.ElapsedTicks);
        }

        #endregion
    }
}

