#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootAsExternalParentCliCommand

    /*
        Another way to create hierarchy between commands, especially if you want to use standalone classes,
        is to use `Parent` property of `CliCommand` attribute to specify `typeof` parent class.
        Command hierarchy in below example is:

         TestApp
         └╴external-level-1-with-parent
           └╴external-level-2-with-parent
    */

    [CliCommand(
        Description = "A root cli command with external children"
    )]
    public class RootAsExternalParentCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; } = "DefaultForArgument1";

        public void Run(CliContext context)
        {
            if (!context.Result.HasArgs)
                context.ShowHierarchy();
            else
                context.ShowValues();
        }
    }

    #endregion
}
