#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands.External
{
    #region ExternalLevel1SubCliCommand

    // Command hierarchy in below example is:  
    // `RootWithExternalChildrenCliCommand` -> `ExternalLevel1SubCliCommand` -> `Level2SubCliCommand`

    [CliCommand(
        Description = "An external level 1 sub-command",
        Parent = typeof(RootWithExternalChildrenCliCommand)
    )]
    public class ExternalLevel1SubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run(CliContext context)
        {
            context.ShowValues();
        }

        [CliCommand(Description = "A nested level 2 sub-command")]
        public class Level2SubCliCommand
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
    }

    #endregion
}
