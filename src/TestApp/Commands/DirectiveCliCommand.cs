#pragma warning disable CS1591
using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region DirectiveCliCommand

    // A root cli command to test directives
    // Currently only `bool`, `string` and `string[]` types are supported for `[CliDirective]` properties.

    [CliCommand(Description = "A root cli command with directives")]
    public class DirectiveCliCommand
    {
        [CliDirective]
        public bool Debug { get; set; }

        [CliDirective]
        public string Directive2 { get; set; }

        [CliDirective]
        public string[] Vars { get; set; }

        public void Run(CliContext context)
        {
            if (context.IsEmpty())
                context.ShowHelp();
            else
            {
                Console.WriteLine($"Directive '{nameof(Debug)}' = {StringExtensions.FormatValue(Debug)}");
                Console.WriteLine($"Directive '{nameof(Directive2)}' = {StringExtensions.FormatValue(Directive2)}");
                Console.WriteLine($"Directive '{nameof(Vars)}' = {StringExtensions.FormatValue(Vars)}");
            }
        }
    }

    #endregion
}
