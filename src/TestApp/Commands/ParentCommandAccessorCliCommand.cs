#pragma warning disable CS1591
using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region ParentCommandAccessorCliCommand

    // Sub-commands can get a reference to the parent command by adding a property of the parent command type.

    [CliCommand(Description = "A root cli command with children that can access parent commands")]
    public class ParentCommandAccessorCliCommand
    {
        [CliOption(
            Description = "This is a global option (Recursive option on the root command), it can appear anywhere on the command line",
            Recursive = true)]
        public string GlobalOption1 { get; set; } = "DefaultForGlobalOption1";

        [CliArgument(Description = "Description for RootArgument1")]
        public string RootArgument1 { get; set; }

        public void Run(CliContext context)
        {
            context.ShowValues();
        }

        [CliCommand(Description = "A nested level 1 sub-command which accesses the root command")]
        public class Level1SubCliCommand
        {
            [CliOption(
                Description = "This is global for all sub commands (it can appear anywhere after the level-1 verb)",
                Recursive = true)]
            public string Level1RecursiveOption1 { get; set; } = "DefaultForLevel1RecusiveOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            // The parent command gets automatically injected
            public ParentCommandAccessorCliCommand RootCommand { get; set; }

            public void Run(CliContext context)
            {
                context.ShowValues();
            }

            [CliCommand(Description = "A nested level 2 sub-command which accesses its parent commands")]
            public class Level2SubCliCommand
            {
                [CliOption(Description = "Description for Option1")]
                public string Option1 { get; set; } = "DefaultForOption1";

                [CliArgument(Description = "Description for Argument1")]
                public string Argument1 { get; set; }

                // All ancestor commands gets injected
                public ParentCommandAccessorCliCommand RootCommand { get; set; }
                public Level1SubCliCommand ParentCommand { get; set; }

                public void Run(CliContext context)
                {
                    context.ShowValues();

                    Console.WriteLine();
                    Console.WriteLine(@$"Level1RecursiveOption1 = {ParentCommand.Level1RecursiveOption1}");
                    Console.WriteLine(@$"parent Argument1 = {ParentCommand.Argument1}");
                    Console.WriteLine(@$"GlobalOption1 = {RootCommand.GlobalOption1}");
                    Console.WriteLine(@$"RootArgument1 = {RootCommand.RootArgument1}");
                }
            }
        }
    }

    #endregion
}
