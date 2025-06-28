#pragma warning disable CS1591
using System;
using System.IO;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    //The other part is in PartialCliCommand.cs

    public partial class PartialCliCommand
    {
        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        [CliOption(Description = "Description for Option2")]
        public string Option2 { get; set; } = "DefaultForOption2";

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }

        [CliCommand]
        internal class PartialNestedCliCommand
        {
            [CliOption]
            public FileInfo InputFile { get; set; } = new FileInfo("default.txt");

            [CliOption]
            public DriveType DriveType { get; set; } = DriveType.Fixed;

            public void Run()
            {
                Console.WriteLine("Running TestNestedPartialCommand");
                Console.WriteLine($"DriveType: {DriveType}");
            }
        }
    }
}
