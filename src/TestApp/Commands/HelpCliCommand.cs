#pragma warning disable CS1591
using System.CommandLine.Invocation;
using DotMake.CommandLine;

namespace TestApp.Commands
{
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

            public void Run(InvocationContext context)
            {
                if (context.IsEmptyCommand())
                    context.ShowHelp();
                else
                    context.ShowValues();
            }
        }
    }
}
