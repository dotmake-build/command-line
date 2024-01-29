#pragma warning disable CS1591
using System;
using System.CommandLine.Invocation;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootWithExternalChildrenCliCommand

    // Another way to create hierarchy between commands, especially if you want to use standalone classes,
    // is to use `Parent` property of `CliCommand` attribute to specify `typeof` parent class.
    // Consider you have this root command:

    [CliCommand(Description = "A root cli command with external children and one nested child and testing settings inheritance")]
    public class RootWithExternalChildrenCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run(InvocationContext context)
        {
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
