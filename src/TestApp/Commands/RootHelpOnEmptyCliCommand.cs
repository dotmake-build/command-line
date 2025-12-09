#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootHelpOnEmptyCliCommand

    // A root cli command which shows help if command is empty, i.e. no arguments or options are passed.
    // Arguments and options should be optional, if they are required (no default values),
    // then handler will not run and missing error message will be shown.

    [CliCommand(Description = "A root cli command")]
    public class RootHelpOnEmptyCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; } = "DefaultForArgument1";

        public void Run(CliContext context)
        {
            if (!context.Result.HasArgs)
                context.ShowHelp();
            else
                context.ShowValues();
        }
    }

    #endregion
}
