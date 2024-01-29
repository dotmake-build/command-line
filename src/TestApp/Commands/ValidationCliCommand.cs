#pragma warning disable CS1591
using System.CommandLine.Invocation;
using System.IO;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region ValidationCliCommand

    // In `[CliOption]` and `[CliArgument]` attributes;
    // `ValidationRules` property allows setting predefined validation rules such as `ExistingFile`, `NonExistingFile`, `ExistingDirectory`,
    // `NonExistingDirectory`, `ExistingFileOrDirectory`, `NonExistingFileOrDirectory`, `LegalPath`, `LegalFileName`, `LegalUri`, `LegalUrl`.
    // Validation rules can be combined.
    // `ValidationPattern` property allows setting a regular expression pattern for custom validation,
    // and `ValidationMessage` property allows setting a custom error message to show when `ValidationPattern` does not match.
    
    [CliCommand]
    public class ValidationCliCommand
    {
        [CliOption(Required = false, ValidationRules = CliValidationRules.ExistingFile)]
        public FileInfo OptFile1 { get; set; }

        [CliOption(Required = false, ValidationRules = CliValidationRules.NonExistingFile | CliValidationRules.LegalPath)]
        public string OptFile2 { get; set; }

        [CliOption(Required = false, ValidationRules = CliValidationRules.ExistingDirectory)]
        public DirectoryInfo OptDir { get; set; }

        [CliOption(Required = false, ValidationPattern = @"(?i)^[a-z]+$", ValidationMessage = null)]
        public string OptPattern1 { get; set; }

        [CliOption(Required = false, ValidationPattern = @"(?i)^[a-z]+$", ValidationMessage = "Custom error message")]
        public string OptPattern2 { get; set; }

        [CliOption(Required = false, ValidationRules = CliValidationRules.LegalUrl)]
        public string OptUrl { get; set; }

        [CliOption(Required = false, ValidationRules = CliValidationRules.LegalUri)]
        public string OptUri { get; set; }

        [CliArgument(Required = false, ValidationRules = CliValidationRules.LegalFileName)]
        public string OptFileName { get; set; }

        public void Run(InvocationContext context)
        {
            context.ShowValues();
        }
    }

    #endregion
}
