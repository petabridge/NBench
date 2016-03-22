using System.Threading.Tasks;

namespace NBench.Tests.Performance
{
    public class SimpleTimingAssertionBenchmark
    {
        [PerfBenchmark(Description = "Spec should take about 100ms to execute", RunMode = RunMode.Iterations, NumberOfIterations = 7)]
        [ElapsedTimeAssertion(MaxTimeMilliseconds = 140, MinTimeMilliseconds = 100)]
        public void ShouldTakeRoughly100ms()
        {
            Task.Delay(100).Wait();
        }
    }
}
