#pragma warning disable CS1591
using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootWithExternalChildrenCliCommand

    // Another way to create hierarchy between commands, especially if you want to use standalone classes,
    // is to use `Parent` property of `CliCommand` attribute to specify `typeof` parent class.
    // Command hierarchy in below example is:
    // RootWithExternalChildrenCliCommand -> ExternalLevel1SubCliCommand -> Level2SubCliCommand

    [CliCommand(
        Description = "A root cli command with external children and one nested child and testing settings inheritance",
        Aliases = new[] { "rootCmdAlias" }
    )]
    public class RootWithExternalChildrenCliCommand
    {
        [CliOption(Description = "Description for Option1", Aliases = new[] { "opt1Alias" })]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliOption(
            Description = "Description for Option2",
            Aliases = new[] { "globalOpt2Alias" },
            Global = true,
            AllowedValues = new[] { "value1", "value2", "value3" }
        )]
        public string Option2 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public int Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();

            return 0;
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
                Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                throw new Exception("This is a test exception");
            }
        }
    }

    #endregion
}
