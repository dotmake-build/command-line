#pragma warning disable CS1591
using System;
using System.Threading.Tasks;
using DotMake.CommandLine;

namespace TestApp.Commands;

[CliCommand(Description = "A root cli command with async handler with Task<int> (return int)")]
public class TaskIntReturnCliCommand
{
    [CliOption(Description = "Description for Option1")]
    public string Option1 { get; set; } = "DefaultForOption1";

    [CliArgument(Description = "Description for Argument1")]
    public string Argument1 { get; set; }

    public Task<int> RunAsync()
    {
        Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
        Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
        Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
        Console.WriteLine();

        return Task.FromResult(0);
    }
}
