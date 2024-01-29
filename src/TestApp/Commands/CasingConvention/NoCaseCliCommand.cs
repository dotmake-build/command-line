#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands.CasingConvention
{
    [CliCommand(
        Description = "A cli command with no case convention",
        NameCasingConvention = CliNameCasingConvention.None
    )]
    public class NoCaseCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }
    }
}
