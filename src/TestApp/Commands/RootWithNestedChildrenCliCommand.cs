#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootWithNestedChildrenCliCommand

    /*
        Defining sub-commands in DotMake.Commandline is very easy. We simply use nested classes to create a hierarchy.
        Just make sure you apply `CliCommand` attribute to the nested classes as well.
        Command hierarchy in below example is:
        
         TestApp
         └╴level-1
           └╴level-2
    */

    [CliCommand(Description = "A root cli command with nested children")]
    public class RootWithNestedChildrenCliCommand
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

        [CliCommand(Description = "A nested level 1 sub-command")]
        public class Level1SubCliCommand
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
    }

    #endregion
}
