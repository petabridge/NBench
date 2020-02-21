using FluentAssertions;
using Xunit;

namespace NBench.Tests
{
    public class CommandLineSpecs
    {
        [Fact(DisplayName = "CommandLine: Should still parse NBench v1.2 commands")]
        public void Should_parse_NBench_10_command_styles()
        {
            var command = new string[] { $"{NBenchCommands.OutputKey}", "D:\\NBench\\PerfResults\\netcoreapp1.0" };
            var dict = NBenchCommands.ParseValues(command);
            dict.ContainsKey(NBenchCommands.OutputKey).Should().BeTrue();
        }
    }
}
