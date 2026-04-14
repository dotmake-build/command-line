#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class InvalidCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliOption(/*Alias = "o1",*/ Description = "Conflicting option")]
        public string Option2 { get; set; }

        public void Run(CliContext context)
        {
            context.ShowValues();
        }

        [CliCommand(Alias = "c", Description = "A nested level 1 sub-command")]
        public class InvalidSubCliCommand
        {
            [CliCommand(Alias = "c", Description = "Conflicting sub-command")]
            public class InvalidSub2Command
            {

            }
        }

        /*
        [CliCommand(Description = "Conflicting sub-command")]
        public class InvalidSubCommand
        {

        }
        */
    }
}
