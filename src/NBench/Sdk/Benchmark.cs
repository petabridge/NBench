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
        private readonly int _warmupCount;
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

        /// <summary>
        /// Backwards-compatible constructor for NBench 0.1.6 and earlier.
        /// </summary>
        /// <param name="settings">The settings for this benchmark.</param>
        /// <param name="invoker">The invoker used to execute benchmark and setup / cleanup methods.</param>
        /// <param name="writer">The output target this benchmark will write to.</param>
        /// <remarks>Uses the <see cref="DefaultBenchmarkAssertionRunner"/> to assert benchmark data.</remarks>
        public Benchmark(BenchmarkSettings settings, IBenchmarkInvoker invoker, IBenchmarkOutput writer) 
            : this(settings, invoker, writer, DefaultBenchmarkAssertionRunner.Instance) { }

        /// <summary>
        /// Backwards-compatible constructor for NBench 0.1.6 and earlier.
        /// </summary>
        /// <param name="settings">The settings for this benchmark.</param>
        /// <param name="invoker">The invoker used to execute benchmark and setup / cleanup methods.</param>
        /// <param name="writer">The output target this benchmark will write to.</param>
        /// <param name="benchmarkAssertions">The assertion engine we'll use to perform BenchmarkAssertions against benchmarks.</param>
        public Benchmark(BenchmarkSettings settings, IBenchmarkInvoker invoker, IBenchmarkOutput writer, IBenchmarkAssertionRunner benchmarkAssertions)
        {
            Settings = settings;
            _warmupCount = _pendingIterations = Settings.NumberOfIterations;
            Invoker = invoker;
            Output = writer;
            CompletedRuns = new Queue<BenchmarkRunReport>(Settings.NumberOfIterations);
            Builder = new BenchmarkBuilder(Settings);
            BenchmarkAssertionRunner = benchmarkAssertions;
        }

        public RunMode RunMode => Settings.RunMode;
        public Queue<BenchmarkRunReport> CompletedRuns { get; }
        public BenchmarkSettings Settings { get; }
        public bool ShutdownCalled { get; private set; }
        protected IBenchmarkInvoker Invoker { get; }
        protected IBenchmarkOutput Output { get; }
        protected IBenchmarkTrace Trace => Settings.Trace;
        protected IBenchmarkAssertionRunner BenchmarkAssertionRunner { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="Finish"/> was called and all BenchmarkAssertions passed,
        /// or if it was never called.
        /// </summary>
        public bool AllAssertsPassed { get; private set; }

        /// <summary>
        /// The name of this benchmark
        /// </summary>
        public string BenchmarkName => Invoker.BenchmarkName;

        /// <summary>
        ///     Warmup phase
        /// </summary>
        private void WarmUp()
        {
            Trace.Debug($"Beginning Warmups for {BenchmarkName}");
            var warmupStopWatch = new Stopwatch();
            var targetTime = Settings.RunTime;
            Contract.Assert(targetTime != TimeSpan.Zero);
            var runCount = 0L;

            /* Pre-Warmup */
           

            Trace.Debug("----- BEGIN PRE-WARMUP -----");
            /* Estimate */
            Allocate(); // allocate all collectors needed
            PreRun();

            try
            {
                if (Settings.RunMode == RunMode.Throughput)
                {
                    Trace.Debug(
                        $"Throughput mode: estimating how many invocations of {BenchmarkName} will take {targetTime.TotalSeconds}s");
                    warmupStopWatch.Start();
                    while (warmupStopWatch.ElapsedTicks < targetTime.Ticks)
                    {
                        Invoker.InvokeRun(_currentRun.Context);
                        runCount++;
                    }
                    warmupStopWatch.Stop();
                    Trace.Debug(
                        $"Throughput mode: executed {runCount} instances of {BenchmarkName} in roughly {targetTime.TotalSeconds}s. Using that figure for benchmark.");
                }
                else
                {
                    warmupStopWatch.Start();
                    Invoker.InvokeRun(_currentRun.Context);
                    runCount++;
                    warmupStopWatch.Stop();
                }
            }
            catch (Exception ex)
            {
                HandleBenchmarkRunException(ex, $"Error occurred during ${BenchmarkName} RUN.");
            }

            PostRun();
            Complete(true);

            // check to see if pre-warmup threw an exception
            var faulted = _currentRun.IsFaulted;

            if (faulted)
            {
                Trace.Error($"Error occurred during pre-warmup. Exiting and producing dump...");
                /*
                 * Normally we don't ever queue up the warmup into the final stats, but we do it
                 * in failure cases so we can capture the exception thrown during warmup into
                 * the final report we're going to deliver to the end-user.
                 */
                CompletedRuns.Enqueue(_currentRun.ToReport(TimeSpan.Zero));

                return;
            }

            Trace.Debug("----- END PRE-WARMUP -----");

            // elapsed time
            var runTime = warmupStopWatch.ElapsedTicks;

            WarmupData = new WarmupData(runTime, runCount);

            Trace.Debug("----- BEGIN WARMUPS -----");
            var i = _warmupCount;

            /* Warmup to force CPU caching */
            while (i > 0 && !_currentRun.IsFaulted)
            {
                RunSingleBenchmark();
                i--;
            }

            Trace.Debug("----- END WARMUPS -----");
        }

        /// <summary>
        ///     Pre-allocate all of the objects we're going to need for this benchmark
        /// </summary>
        private void Allocate()
        {
            _currentRun = Builder.NewRun(WarmupData);
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
                HandleBenchmarkRunException(ex, $"Error occurred during ${BenchmarkName} RUN.");
            }
            PostRun();
            Complete(); // release previous collectors
        }

        protected void PreRun()
        {
            Trace.Info($"Invoking setup for {BenchmarkName}");
            try
            {
                if (RunMode == RunMode.Throughput)
                {
                    // Need to pass in the # of estimated runs per second in order to compile
                    // the invoker with an inlined loop
                    Invoker.InvokePerfSetup(WarmupData.EstimatedRunsPerSecond, _currentRun.Context);
                }
                    else
                {
                    // Invoke user-defined setup method, if any
                    Invoker.InvokePerfSetup(_currentRun.Context);
                }
                

                PrepareForRun();
            }
            catch (Exception ex)
            {
                HandleBenchmarkRunException(ex, $"Error occurred during ${BenchmarkName} SETUP.");
            }
        }

        private void HandleBenchmarkRunException(Exception ex, string formatMsg)
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
            Trace.Info($"Invoking {BenchmarkName}");
            _currentRun.Sample(StopWatch.ElapsedTicks);
            StopWatch.Start();
            Invoker.InvokeRun(_currentRun.Context);
            StopWatch.Stop();
            _currentRun.Sample(StopWatch.ElapsedTicks); //add a tick just to ensure no collision
        }

        /// <summary>
        ///     NOTE: We don't reset the <see cref="Stopwatch" /> on purpose here, so we can
        ///     collect the value of it and use it for auto-tuning and reporting.
        ///     It'll be started at the beginning of the next run.
        /// </summary>
        protected void PostRun()
        {
            Trace.Info($"Invoking cleanup for {BenchmarkName}");
            try
            {
                // Invoke user-defined cleanup method, if any
                Invoker.InvokePerfCleanup(_currentRun.Context);
            }
            catch (Exception ex)
            {
                HandleBenchmarkRunException(ex, $"Error occurred during ${BenchmarkName} CLEANUP.");
            }
        }

        /// <summary>
        /// Helper method for printing out the correct label to trace
        /// </summary>
        private static string PrintWarmupOrRun(bool isWarmup)
        {
            return isWarmup ? "warmup" : "run";
        }

        /// <summary>
        /// Complete the current run
        /// </summary>
        private void Complete(bool isEstimate = false)
        {
            _currentRun.Dispose();
            Trace.Info($"Generating report for {PrintWarmupOrRun(_isWarmup)} {1 + Settings.NumberOfIterations - _pendingIterations} of {BenchmarkName}");
            var report = _currentRun.ToReport(StopWatch.Elapsed);
            if(!isEstimate)
                Output.WriteRun(report, _isWarmup);

            // Change runs, but not on warmup
            if (!_isWarmup)
            {
                // Decrease the number of pending iterations
                _pendingIterations--;


                CompletedRuns.Enqueue(report);
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
            var assertionResults = BenchmarkAssertionRunner.RunAssertions(Settings, result);
            return new BenchmarkFinalResults(result, assertionResults);
        }

        /// <summary>
        /// Complete the benchmark
        /// </summary>
        public void Finish()
        {
            var results = CompileResults();

            var finalResults = AssertResults(results);
            AllAssertsPassed = finalResults.AssertionResults.All(x => x.Passed) && !finalResults.Data.IsFaulted;
            Output.WriteBenchmark(finalResults);
        }
    }
}

