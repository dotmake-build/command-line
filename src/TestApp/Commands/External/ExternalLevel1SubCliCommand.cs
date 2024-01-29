#pragma warning disable CS1591
using System.CommandLine.Invocation;
using DotMake.CommandLine;

namespace TestApp.Commands.External
{
    [CliCommand(
        Name = "Level1External",
        Description = "An external level 1 sub-command",
        Parent = typeof(RootWithExternalChildrenCliCommand)
    )]
    public class ExternalLevel1SubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run(InvocationContext context)
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

            public void Run(InvocationContext context)
            {
                context.ShowValues();
            }
        }
    }
}
