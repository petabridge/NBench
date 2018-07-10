using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace NBench.Execution.Tests
{
    public class CommandLineSpecs
    {
        [Fact(DisplayName = "CommandLine: Should still parse NBench v1.0.* commands")]
        public void Should_parse_NBench_10_command_styles()
        {
            var command = new string[] { $"{CommandLine.OutputKey}=foo" };
            var dict = CommandLine.ParseValues(command);
            dict.ContainsKey(CommandLine.OutputKey).Should().BeTrue();
        }
    }
}
