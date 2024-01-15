#pragma warning disable CS1591
#nullable enable
using System;
using System.IO;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand(Description = "A cli command with input (must exists) files and or directories")]
    public class FileSystemInfoCliCommand
    {
        [CliOption(Description = "Optional input file (must exists)", ExistingOnly = true)]
        public FileInfo? Option1 { get; set; } = null;

        [CliOption(Description = "Optional input directory (must exists)", ExistingOnly = true)]
        public DirectoryInfo? Option2 { get; set; } = null;

        [CliArgument(Description = "Input file or directory (must exists)", ExistingOnly = true)]
        public FileSystemInfo Argument1 { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Option2)} property is '{Option2}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }
    }
}
