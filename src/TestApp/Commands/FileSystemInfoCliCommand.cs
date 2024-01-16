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
        [CliOption(Description = "Optional input file (must exists)", AllowExisting = true, Required = false)]
        public FileInfo? FileOpt { get; set; }

        [CliOption(Description = "Optional input directory (must exists)", AllowExisting = true, Required = false)]
        public DirectoryInfo? DirOpt { get; set; } 

        [CliArgument(Description = "Input file or directory (must exists)", AllowExisting = true)]
        public required FileSystemInfo FileOrDirArg { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(FileOpt)} property is '{FileOpt}'");
            Console.WriteLine($@"Value for {nameof(DirOpt)} property is '{DirOpt}'");
            Console.WriteLine($@"Value for {nameof(FileOrDirArg)} property is '{FileOrDirArg}'");
            Console.WriteLine();
        }
    }
}
