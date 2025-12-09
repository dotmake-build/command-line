#pragma warning disable CS1591
using System;
using System.IO;
using DotMake.CommandLine;
using TestApp.Commands.OtherNamespace;

namespace TestApp.Commands
{
    #region DefaultValuesCliCommand

    [CliCommand(Description = "A root cli command for testing various property initializers (default values)")]
    public class DefaultValuesCliCommand
    {
        [CliArgument]
        public string Arg1 { get; set; } = "Arg1Value";

        [CliArgument]
        public int Arg2 { get; set; } = 2;

        [CliOption]
        public string Opt1 { get; set; } = "Opt1Value";

        [CliOption]
        public int Opt2 { get; set; } = 5;

        [CliOption]
        public FileAccess Opt3 { get; set; } = FileAccess.Read;

        [CliOption]
        public FileAccess Opt4 { get; set; } = GetFileAccess();

        [CliOption]
        public FileAccess Opt5 { get; set; } = StaticFileAccess;

        [CliOption]
        public bool Opt6 { get; set; } = true;

        [CliOption]
        public string Opt8 { get; set; } = @"value with
            new line";

        [CliOption]
        public string Opt9 { get; set; } = $"value {StaticFileAccess}";

        [CliOption]
        public CustomClass[] TestOpt { get; set; } = Array.Empty<CustomClass>();

        [CliOption] public string Opt10 { get; set; } = new ('-', 5);

        public static FileAccess GetFileAccess()
        {
            return FileAccess.Write;
        }

        public static FileAccess StaticFileAccess = FileAccess.ReadWrite;

        public void Run(CliContext context)
        {
            context.ShowValues();
        }
    }

    #endregion

    namespace OtherNamespace
    {
        public class CustomClass
        {
            // ReSharper disable once UnusedParameter.Local
            public CustomClass(string value)
            {
            }
        }
    }
}
