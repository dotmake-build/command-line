#pragma warning disable CS1591
using System;
using System.Threading.Tasks;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RecursiveOptionCliCommand

    // Getting the value for a recursive option in a sub-command:

    [CliCommand(Description = "A root cli command")]
    public class RecursiveOptionCliCommand
    {
        [CliOption(Recursive = true)]
        public bool RecursiveOption { get; set; }

        [CliCommand]
        public class SubCliCommand
        {
            [CliArgument]
            public string Argument1 { get; set; } = "DefaultForArgument1";

            public async Task RunAsync(CliContext context)
            {
                var parent = context.ParseResult.Bind<RecursiveOptionCliCommand>();

                await Console.Out.WriteLineAsync($"RecursiveOption = {parent.RecursiveOption}, Argument1 = {Argument1}");
            }
        }
    }

    #endregion
}
