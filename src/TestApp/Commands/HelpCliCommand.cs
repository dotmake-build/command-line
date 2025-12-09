#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region HelpCliCommand

    // If a command represents a group and not an action, you may want to show help.
    // If `Run` or `RunAsync` method is missing in a command class, then by default it will show help.
    // You can also manually trigger help in `Run` or `RunAsync` method of a command class via calling `CliContext.ShowHelp`.
    // For testing a command, other methods `CliContext.ShowValues` and `CliContext.IsEmptyCommand` are also useful.
    // `ShowValues` shows parsed values for current command and its arguments and options.

    // See below example; root command does not have a handler method so it will always show help
    // and sub-command will show help if command is specified without any arguments or option,
    // and it will show(dump) values if not:

    [CliCommand(Description = "A root cli command")]
    public class HelpCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; } = "DefaultForArgument1";

        [CliCommand(Description = "A sub cli command")]
        public class SubCliCommand
        {
            [CliArgument(Description = "Description for Argument2")]
            public string Argument2 { get; set; } = "DefaultForArgument2";

            public void Run(CliContext context)
            {
                if (!context.Result.HasArgs)
                    context.ShowHelp();
                else
                    context.ShowValues();
            }
        }
    }

    #endregion
}
