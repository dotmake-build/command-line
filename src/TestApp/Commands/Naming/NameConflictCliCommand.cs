#pragma warning disable CS1591
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class NameConflictCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "";

        [CliOption( /*Alias = "o1",*/ Description = "Conflicting option")]
        public string Option2 { get; set; } = "";

        public void Run(CliContext context)
        {
            context.ShowValues();
        }

        [CliCommand(Description = "Conflicting with root command")]
        public class NameConflictCommand
        {

        }

        [CliCommand(Description = "Conflicting with root executable name")]
        public class TestAppCommand
        {

        }

        [CliCommand(Alias = "c", Description = "A nested level 1 sub-command")]
        public class InvalidSubCliCommand
        {
            [CliCommand(Alias = "c2", Description = "Conflicting sub-command")]
            public class InvalidSubCommand
            {

            }
        }
        
        [CliCommand(Description = "Conflicting sub-command")]
        public class InvalidSubCommand
        {

        }

        [CliCommand(Name = "bitbucket", Description = "Bitbucket related")]
        public class BitbucketCommand
        {
            [CliCommand(Name = "build", Description = "Trigger pipeline build for component")]
            public class BuildCommand {}
        }
       
    }
}
