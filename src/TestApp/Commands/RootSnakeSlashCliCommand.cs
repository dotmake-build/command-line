#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootSnakeSlashCliCommand

    // For example, change the name casing and prefix convention:

    [CliCommand(
        Description = "A cli command with snake_case name casing and forward slash prefix conventions",
        NameCasingConvention = CliNameCasingConvention.SnakeCase,
        NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
        ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash
    )]
    public class RootSnakeSlashCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }
    }

    #endregion
}
