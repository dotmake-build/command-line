#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region DerivedLocalizedCliCommand

    // BaseLocalizedCliCommand is located in a separate file to test if inherited properties are also localized without errors (issue#71).

    [CliCommand(Description = nameof(TestResources.CommandDescription))]
    public class DerivedLocalizedCliCommand : BaseLocalizedCliCommand
    {
        [CliOption(Description = nameof(TestResources.OptionDescription))]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = nameof(TestResources.ArgumentDescription))]
        public string Argument1 { get; set; }
    }

    #endregion
}
