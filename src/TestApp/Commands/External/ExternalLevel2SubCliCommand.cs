#pragma warning disable CS1591
using System.CommandLine.Invocation;
using DotMake.CommandLine;

namespace TestApp.Commands.External
{
    #region ExternalLevel1SubCliCommand

    // Command hierarchy in below example is:  
    // `RootWithExternalChildrenCliCommand` -> `Level1SubCliCommand` -> `ExternalLevel2SubCliCommand` -> `Level3SubCliCommand`

    [CliCommand(
        Description = "An external level 2 sub-command",
        Parent = typeof(RootWithExternalChildrenCliCommand.Level1SubCliCommand),
        NameCasingConvention = CliNameCasingConvention.SnakeCase,
        NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
        ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash
    )]
    public class ExternalLevel2SubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run(InvocationContext context)
        {
            context.ShowValues();
        }

        [CliCommand(Description = "A nested level 3 sub-command")]
        public class Level3SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            public void Run(InvocationContext context)
            {
                context.ShowValues();
            }
        }
    }

    #endregion
}
