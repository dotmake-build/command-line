using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Text.RegularExpressions;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides extension methods related to validation for <see cref="Argument"/> and <see cref="Option"/>. 
    /// </summary>
    public static class CliValidationExtensions
    {
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        /// <summary>
        /// Adds validation rules to an option's argument.
        /// Validation rules can be used to provide errors based on user input.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="validationRules">The validation rules to add.</param>
        public static void AddValidator(this Option option, CliValidationRules validationRules)
        {
            AddValidator(option.GetArgument(), validationRules);
        }
        
        /// <summary>
        /// Adds validation rules to an argument.
        /// Validation rules can be used to provide errors based on user input.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <param name="validationRules">The validation rules to add.</param>
        public static void AddValidator(this Argument argument, CliValidationRules validationRules)
        {
            if (validationRules.HasFlag(CliValidationRules.ExistingFile))
                argument.AddValidator(static result => ValidateArgumentResult(result, ExistingFile));

            if (validationRules.HasFlag(CliValidationRules.NonExistingFile))
                argument.AddValidator(static result => ValidateArgumentResult(result, NonExistingFile));

            if (validationRules.HasFlag(CliValidationRules.ExistingDirectory))
                argument.AddValidator(static result => ValidateArgumentResult(result, ExistingDirectory));

            if (validationRules.HasFlag(CliValidationRules.NonExistingDirectory))
                argument.AddValidator(static result => ValidateArgumentResult(result, NonExistingDirectory));

            if (validationRules.HasFlag(CliValidationRules.ExistingFileOrDirectory))
                argument.AddValidator(static result => ValidateArgumentResult(result, ExistingFileOrDirectory));

            if (validationRules.HasFlag(CliValidationRules.NonExistingFileOrDirectory))
                argument.AddValidator(static result => ValidateArgumentResult(result, NonExistingFileOrDirectory));

            if (validationRules.HasFlag(CliValidationRules.LegalPath))
                argument.AddValidator(static result => ValidateArgumentResult(result, LegalPath));

            if (validationRules.HasFlag(CliValidationRules.LegalFileName))
                argument.AddValidator(static result => ValidateArgumentResult(result, LegalFileName));

            if (validationRules.HasFlag(CliValidationRules.LegalUri))
                argument.AddValidator(static result => ValidateArgumentResult(result, LegalUri));

            if (validationRules.HasFlag(CliValidationRules.LegalUrl))
                argument.AddValidator(static result => ValidateArgumentResult(result, LegalUrl));
        }

        /// <summary>
        /// Adds a validation pattern to an option's argument.
        /// A validation pattern can be used to provide errors based on user input.
        /// </summary>
        /// <param name="option">The option to configure.</param>
        /// <param name="validationPattern">A regular expression pattern used for validation.</param>
        /// <param name="validationMessage">An error message to show when <paramref name="validationPattern"/> does not match and validation fails.</param>
        public static void AddValidator(this Option option, string validationPattern, string validationMessage = null)
        {
            AddValidator(option.GetArgument(), validationPattern, validationMessage);
        }

        /// <summary>
        /// Adds a validation pattern to an argument.
        /// A validation pattern can be used to provide errors based on user input.
        /// </summary>
        /// <param name="argument">The argument to configure.</param>
        /// <param name="validationPattern">A regular expression pattern used for validation.</param>
        /// <param name="validationMessage">An error message to show when <paramref name="validationPattern"/> does not match and validation fails.</param>
        public static void AddValidator(this Argument argument, string validationPattern, string validationMessage = null)
        {
            if (!string.IsNullOrWhiteSpace(validationPattern))
                argument.AddValidator(result =>
                    ValidateArgumentResult(result, validationResult => RegularExpression(validationResult, validationPattern, validationMessage))
                );
        }

        private class ValidationResult
        {
            public ValidationResult(string value, LocalizationResources localizationResources)
            {
                Value = value;
                LocalizationResources = localizationResources;
            }

            public string Value { get; }

            public LocalizationResources LocalizationResources { get; }

            public bool Success { get; set; }

            // ReSharper disable once InconsistentNaming
            public string ErrorMessage { get; set; }
        }

        private static void ValidateArgumentResult(ArgumentResult result, Action<ValidationResult> validateToken)
        {
            foreach (var token in result.Tokens)
            {
                var validationResult = new ValidationResult(token.Value, result.LocalizationResources);

                validateToken(validationResult);

                if (!validationResult.Success)
                {
                    if (result.Parent is OptionResult optionResult)
                        result.ErrorMessage = $"Invalid argument for option '{optionResult.Token}'";
                    else if (result.Parent is CommandResult commandResult)
                        result.ErrorMessage = $"Invalid argument for command '{commandResult.Token}'";

                    if (!string.IsNullOrEmpty(validationResult.ErrorMessage))
                        result.ErrorMessage += " -> " + validationResult.ErrorMessage;

                    return;
                }
            }
        }

        private static void ExistingFile(ValidationResult validationResult)
        {
            if (!File.Exists(validationResult.Value))
            {
                validationResult.ErrorMessage = validationResult.LocalizationResources.FileDoesNotExist(validationResult.Value);
                return;
            }

            validationResult.Success = true;
        }

        private static void NonExistingFile(ValidationResult validationResult)
        {
            if (File.Exists(validationResult.Value))
            {
                validationResult.ErrorMessage = $"File already exists: '{validationResult.Value}'.";
                return;
            }

            validationResult.Success = true;
        }

        private static void ExistingDirectory(ValidationResult validationResult)
        {
            if (!Directory.Exists(validationResult.Value))
            {
                validationResult.ErrorMessage = validationResult.LocalizationResources.DirectoryDoesNotExist(validationResult.Value);
                return;
            }

            validationResult.Success = true;
        }

        private static void NonExistingDirectory(ValidationResult validationResult)
        {
            if (Directory.Exists(validationResult.Value))
            {
                validationResult.ErrorMessage = $"Directory already exists: '{validationResult.Value}'.";
                return;
            }

            validationResult.Success = true;
        }

        private static void ExistingFileOrDirectory(ValidationResult validationResult)
        {
            if (!Directory.Exists(validationResult.Value) && !File.Exists(validationResult.Value))
            {
                validationResult.ErrorMessage = validationResult.LocalizationResources.FileOrDirectoryDoesNotExist(validationResult.Value);
                return;
            }

            validationResult.Success = true;
        }

        private static void NonExistingFileOrDirectory(ValidationResult validationResult)
        {
            if (Directory.Exists(validationResult.Value) || File.Exists(validationResult.Value))
            {
                validationResult.ErrorMessage = $"File or directory already exists: '{validationResult.Value}'.";
                return;
            }

            validationResult.Success = true;
        }

        private static void LegalPath(ValidationResult validationResult)
        {
            if (string.IsNullOrWhiteSpace(validationResult.Value))
            {
                validationResult.ErrorMessage = "Path cannot be the empty string or all whitespace.";
                return;
            }

            var invalidCharactersIndex = validationResult.Value.IndexOfAny(InvalidPathChars);

            if (invalidCharactersIndex >= 0)
            {
                validationResult.ErrorMessage = validationResult.LocalizationResources.InvalidCharactersInPath(validationResult.Value[invalidCharactersIndex]);
                return;
            }

            var path = validationResult.Value;
            var root = Path.GetPathRoot(path) ?? string.Empty;
            var parts = path.Substring(root.Length)
                .Split(new []{ Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var partValidationResult = new ValidationResult(part, validationResult.LocalizationResources);

                LegalFileName(partValidationResult);

                if (!partValidationResult.Success)
                {
                    validationResult.ErrorMessage = partValidationResult.ErrorMessage;
                    return;
                }
            }

            validationResult.Success = true;
        }

        private static void LegalFileName(ValidationResult validationResult)
        {
            if (string.IsNullOrWhiteSpace(validationResult.Value))
            {
                validationResult.ErrorMessage = "File name cannot be the empty string or all whitespace.";
                return;
            }

            var invalidCharactersIndex = validationResult.Value.IndexOfAny(InvalidFileNameChars);

            if (invalidCharactersIndex >= 0)
            {
                validationResult.ErrorMessage = validationResult.LocalizationResources.InvalidCharactersInFileName(validationResult.Value[invalidCharactersIndex]);
                return;
            }

            validationResult.Success = true;
        }

        private static void LegalUri(ValidationResult validationResult)
        {
            if (string.IsNullOrWhiteSpace(validationResult.Value))
            {
                validationResult.ErrorMessage = "URI cannot be the empty string or all whitespace.";
                return;
            }

            try
            {
                // Valid examples:
                // http://www.google.com
                // ftp://ftp.is.co.za/rfc/rfc1808.txt 
                // file:///c:/directory/filename

                var unused = new Uri(validationResult.Value, UriKind.Absolute);
            }
            catch (Exception exception)
            {
                validationResult.ErrorMessage = $"{exception.Message.TrimEnd('.')}: '{validationResult.Value}'.";
                return;

            }

            validationResult.Success = true;
        }

        private static void LegalUrl(ValidationResult validationResult)
        {
            if (string.IsNullOrWhiteSpace(validationResult.Value))
            {
                validationResult.ErrorMessage = "URL cannot be the empty string or all whitespace.";
                return;
            }

            try
            {
                // Valid examples:
                // https://www.google.com
                // http://www.google.com
                // www.google.com
                // google.com

                var uri = new Uri(validationResult.Value, UriKind.RelativeOrAbsolute);

                if (uri.IsAbsoluteUri //Scheme will throw if not absolute
                    && uri.Scheme != Uri.UriSchemeHttp
                    && uri.Scheme != Uri.UriSchemeHttps)
                    throw new Exception("URL must have http or https scheme when it's absolute.");

                //Allow relative url to consider it as starting with http://
            }
            catch (Exception exception)
            {
                validationResult.ErrorMessage = $"{exception.Message.TrimEnd('.')}: '{validationResult.Value}'.";
                return;
            }

            validationResult.Success = true;
        }

        private static void RegularExpression(ValidationResult validationResult, string validationPattern, string validationMessage)
        {
            if (!Regex.IsMatch(validationResult.Value, validationPattern)) //Regex.IsMatch already caches the regex
            {
                validationResult.ErrorMessage = string.IsNullOrWhiteSpace(validationMessage)
                    ? $"Validation pattern does not match: '{validationResult.Value}'."
                    : $"{validationMessage.TrimEnd('.')}: '{validationResult.Value}'.";
                return;
            }

            validationResult.Success = true;
        }
    }
}
