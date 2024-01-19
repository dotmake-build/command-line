using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand(Description = nameof(TestResources.CommandDescription))]
    internal class LocalizedCliCommand
    {
        [CliOption(Description = nameof(TestResources.OptionDescription))]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = nameof(TestResources.ArgumentDescription))]
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
