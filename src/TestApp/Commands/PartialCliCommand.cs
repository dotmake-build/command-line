#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    //The other part is in PartialCliCommand2.cs

    [CliCommand(Description = "A cli command with partial class")]
    public partial class PartialCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";
    }
}
