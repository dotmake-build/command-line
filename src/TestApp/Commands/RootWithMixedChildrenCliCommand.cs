#pragma warning disable CS1591
using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootWithMixedChildrenCliCommand

    /*
        Another way to create hierarchy between commands, especially if you want to use standalone classes,
        is to use `Parent` property of `CliCommand` attribute to specify `typeof` parent class.
        Command hierarchy in below example is:

         TestApp
         ├╴level_1
         │ └╴external_level_2_with_nested
         │   └╴level_3
         └╴external-level-1-with-nested
           └╴level-2
    */

    [CliCommand(Description = "A root cli command with external children and one nested child and testing settings inheritance")]
    public class RootWithMixedChildrenCliCommand
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

        [CliCommand(
            Description = "A nested level 1 sub-command with custom settings, throws test exception",
            NameCasingConvention = CliNameCasingConvention.SnakeCase,
            NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
            ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash
        )]
        public class Level1SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            public void Run()
            {
                throw new Exception("This is a test exception from Level1SubCliCommand");
            }
        }
    }

    #endregion
}
