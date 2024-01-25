#pragma warning disable CS1591
#nullable enable
using System.CommandLine.Invocation;
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

        public void Run(InvocationContext context)
        {
            context.ShowValues();
        }
    }
}
