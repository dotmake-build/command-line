using System;
using DotMake.CommandLine;

namespace TestApp.Commands.External
{
    public class NestedExternalContainer
    {
        [CliCommand(
            Description = "A nested level 2 sub-command inside a non-command class (currently this is ignored)",
            Parent = typeof(RootWithExternalChildrenCliCommand)
        )]
        public class NestedLevel2SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; } = "DefaultForArgument1";

            public void Run()
            {
                Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
                Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
                Console.WriteLine();
            }
        }
    }
}
