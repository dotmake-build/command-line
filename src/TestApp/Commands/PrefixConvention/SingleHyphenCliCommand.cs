#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands.PrefixConvention
{
    [CliCommand(
        Description = "A cli command with single hyphen prefix convention",
        NamePrefixConvention = CliNamePrefixConvention.SingleHyphen
    )]
    public class SingleHyphenCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }
    }
}
