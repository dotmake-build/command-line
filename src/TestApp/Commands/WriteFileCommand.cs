#pragma warning disable CS1591
using System.Collections.Generic;
using System.IO;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region WriteFileCommand

    // Note that you can have a specific type (other than `string`) for a property which a `CliOption` or `CliArgument`
    // attribute is applied to, for example these properties will be parsed and bound/populated automatically:

    [CliCommand]
    public class WriteFileCommand
    {
        [CliArgument]
        public FileInfo OutputFile { get; set; }

        [CliOption]
        public List<string> Lines { get; set; }

        public void Run()
        {
            if (OutputFile.Exists)
                return;

            using (var streamWriter = OutputFile.CreateText())
            {
                foreach (var line in Lines)
                {
                    streamWriter.WriteLine(line);
                }
            }
        }
    }

    #endregion
}
