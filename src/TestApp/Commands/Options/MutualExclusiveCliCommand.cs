#pragma warning disable CS1591
using DotMake.CommandLine;


namespace TestApp.Commands
{   
    // This class demonstrates the use of mutually exclusive options in a CLI application.

    [CliCommand]
    public class MutualExclusiveCliCommand
    {
        #region FormatCommand

        // The FormatCommand demonstrates basic usage of mutually exclusive grouped options.
        [CliCommand(Description = "Display different file formats")]
        public class FormatCommand
        {
            //Group Format mutually exclusive
            [CliOption(Group = "Format", Description = "Output as XML")]
            public bool Xml { get; set; }

            [CliOption(Group = "Format", Description = "Output as JSON")]
            public bool Json { get; set; }

            [CliOption(Description = "Verbosity level", Required = false)]
            public string Verbose { get; set; }

            public void Run(CliContext context)
            {
                context.ShowValues();
            }
        }

        #endregion

        #region ReportCommand

        // The ReportCommand allows users to generate reports in different formats.
        // It enforces two rules:
        // 1. Output format options (--json, --xml) are mutually exclusive.
        // 2. Authentication options (--apikey, --token) are required: exactly one must be provided.
        [CliCommand(
            Description = "Generate a report with a chosen output format. Requires authentication via API key or token.",
            RequiredGroups = new[] { "auth" }
        )]
        public class ReportCommand
        {
            // Group 1: Output format (mutually exclusive)
            [CliOption(
                Group = "output-format",
                Description = "Output the report in JSON format."
            )]
            public bool Json { get; set; }

            [CliOption(
                Group = "output-format",
                Description = "Output the report in XML format."
            )]
            public bool Xml { get; set; }

            // Group 2: Authentication (required group)
            [CliOption(
                Group = "auth",
                Description = "Authenticate using an API key (exactly one of --apikey or --token must be provided)."
            )]
            public string ApiKey { get; set; }

            [CliOption(
                Group = "auth",
                Description = "Authenticate using a bearer token (exactly one of --apikey or --token must be provided)."
            )]
            public string Token { get; set; }

            // Execution logic
            public void Run(CliContext context)
            {
                // Display parsed values for demonstration purposes
                context.ShowValues();

                // Example: Here you could add logic to generate the report
                // based on the selected format and authentication method.
            }
        }

        #endregion
    }
}
