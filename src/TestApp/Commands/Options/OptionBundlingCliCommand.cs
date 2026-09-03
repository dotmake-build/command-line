#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region OptionBundlingCliCommand

    // Bundling of single-character options are supported, also known as stacking.
    // Bundled options are single-character option aliases specified together after a single hyphen prefix.
    // For example if you have options "-a", "-b" and "-c", you can bundle them like "-abc".
    // Only the last option can specify an argument.
    // Note that if you have an explicit option named "-abc" then it will win over bundled options.

    [CliCommand]
    public class OptionBundlingCliCommand
    {
        [CliOption(Name = "-a")]
        public bool A { get; set; }

        [CliOption(Name = "-b")]
        public bool B { get; set; }

        [CliOption(Name = "-c")]
        public bool C { get; set; }

        public void Run(CliContext cliContext)
        {
            cliContext.ShowValues();
        }
    }

    #endregion
}
