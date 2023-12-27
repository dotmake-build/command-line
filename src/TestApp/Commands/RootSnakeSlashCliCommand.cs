using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand(
        Description = "A cli command with snake_case name casing and forward slash prefix conventions",
        NameCasingConvention = CliNameCasingConvention.SnakeCase,
        NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
        ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash
    )]
    public class RootSnakeSlashCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }
    }
}
