#pragma warning disable CS1591
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

        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        public void Run(CliContext context)
        {
            if (!context.Result.HasTokens)
                context.ShowHelp();
            else
                context.ShowValues();
        }

        [CliCommand(Description = "A sub cli command with directives")]
        public class Level1CliCommand
        {
            public DirectiveCliCommand Parent { get; set; }

            public void Run(CliContext context)
            {
                if (!context.Result.HasTokens)
                    context.ShowHelp();
                else
                    context.ShowValues();
            }
        }
    }

    #endregion
}
