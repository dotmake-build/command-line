#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region OrderedCliCommand

    [CliCommand(Description = "A root cli command with custom order")]
    public class OrderedCliCommand
    {
        [CliOption(Description = "Description for Option1", Order = 2)]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliOption(Description = "Description for Option2", Order = 1)]
        public string Option2 { get; set; } = "DefaultForOption2";

        [CliArgument(Description = "Description for Argument1", Order = 2)]
        public string Argument1 { get; set; } = "DefaultForArgument1";

        [CliArgument(Description = "Description for Argument2", Order = 1)]
        public string Argument2 { get; set; } = "DefaultForArgument2";

        [CliCommand(Description = "Description for sub-command1", Order = 2)]
        public class Sub1CliCommand
        {

        }

        [CliCommand(Description = "Description for sub-command2", Order = 1)]
        public class Sub2CliCommand
        {

        }
    }

    #endregion
}
