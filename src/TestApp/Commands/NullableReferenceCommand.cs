#pragma warning disable CS1591
#nullable enable
using System;
using System.CommandLine.Invocation;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class NullableReferenceCommand
    {
        [CliOption(
            Description = "Description for Display",
            AllowedValues = new[] { "Big", "Small" },
            Required = true
        )]
        public string? Display { get; set; } //= "test";

        [CliArgument(Required = false)]
        public string[]? NullableRefArg { get; set; } //= new[] { "1", "2" };

        public void Run(InvocationContext context)
        {
            Console.WriteLine(Display);
        }
    }
}
