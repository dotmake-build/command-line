#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region RootHelpOnEmptyCliCommand

    // A root cli command to test short form aliases and names

    [CliCommand(
        //NameAutoGenerate = CliNameAutoGenerate.Options
        //,ShortFormAutoGenerate = CliNameAutoGenerate.Options
    )]
    public class ShortFormCliCommand
    {
        [CliOption(Alias = "o2")]
        public string Oauth2GrantType { get; set; } = "";

        [CliOption]
        public string Oauth2TokenUrl { get; set; } = "";

        [CliOption]
        public string Oauth2ClientId { get; set; } = "";

        [CliOption]
        public string Oauth2ClientSecret { get; set; } = "";

        [CliOption]
        public string Sha256 { get; set; } = "";

        [CliOption(Alias = null, Aliases = new []{" ", "opt1"})]
        public string Option1 { get; set; } = "";

        [CliOption(Aliases = null)]
        public string Option1Option { get; set; } = "";

        [CliOption(Alias = "-o2o", Aliases = new[] { "/opt2", null })]
        public string Option2 { get; set; } = "";

        [CliOption(Name = "opt3", Alias = "/opt3", Aliases = new[] { "-opt3", null })]
        public string Option3 { get; set; } = "";

        [CliArgument]
        public string Argument1 { get; set; } = "";

        [CliArgument]
        public string Argument1Argument { get; set; } = "";

        [CliArgument]
        public string Option1Argument { get; set; } = "";


        [CliCommand]
        public class TestCommand
        {

        }

        [CliCommand(Aliases = new []{"op1"} /*, Name = "-o2"*/)]
        public class Option1Command
        {
            
        }

        [CliCommand(Name = "cmd", Alias = "cl")]
        public class CliCommand
        {

        }

        public void Run(CliContext context)
        {
            if (context.IsEmptyCommand())
                context.ShowHelp();
            else
                context.ShowValues();
        }
    }

    #endregion
}
