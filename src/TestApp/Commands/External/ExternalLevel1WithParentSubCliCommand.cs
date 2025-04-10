#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands.External
{
    #region ExternalLevel1WithParentSubCliCommand

    [CliCommand(
        Description = "An external level 1 sub-command",
        Parent = typeof(RootAsExternalParentCliCommand)
    )]
    public class ExternalLevel1WithParentSubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run(CliContext context)
        {
            context.ShowValues();
        }
    }

    #endregion
}
