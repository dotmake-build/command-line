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

        [CliOption(
            Description = "Description for Display2",
            AllowedValues = new[] { "Big", "Small" }
        )]
        public string Display2 { get; set; } = null!;

        [CliArgument(Required = false)]
        public string[]? NullableRefArg { get; set; } //= new[] { "1", "2" };

        [CliOption]
        public string ReqOption { get; set; } = null!;

        [CliArgument]
        public string ReqArg { get; set; } = null!;

        public void Run(InvocationContext context)
        {
            Console.WriteLine(Display);
        }
    }
}
