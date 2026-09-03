#pragma warning disable CS1591
#nullable enable
using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class NullableReferenceCommand
    {
        public void Run()
        {
            Console.WriteLine(@"Please specify a subcommand!");
        }

        [CliCommand]
        public class NullableCommand
        {
            [CliOption(
                Description = "Option with nullable reference type with no default value (should be required)",
                AllowedValues = new[] { "Big", "Small" }
            )]
            public string? Opt { get; set; }

            [CliOption(
                Description = "Option with nullable reference type with default value (should not be required)",
                AllowedValues = new[] { "Big", "Small" }
            )]
            public string? OptDefault { get; set; } = "Big";

            [CliArgument(Description = "Argument with nullable reference type with no default value (should be required)")]
            public string? Arg { get; set; }

            [CliArgument(Description = "Argument with nullable reference type with default value (should not be required)")]
            public string? ArgDefault { get; set; } = "test";

            public void Run(CliContext context)
            {
                context.ShowValues();
            }
        }

        [CliCommand]
        public class NonNullableCommand
        {
            [CliOption(
                Description = "Option with non-nullable reference type with SuppressNullableWarningExpression (should be required)",
                AllowedValues = new[] { "Big", "Small" }
            )]
            public string Opt { get; set; } = null!;

            [CliOption(
                Description = "Option with non-nullable reference type with default value (should not be required)",
                AllowedValues = new[] { "Big", "Small" }
            )]
            public string OptDefault { get; set; } = "Big";

            [CliArgument(Description = "Argument with non-nullable reference type type with SuppressNullableWarningExpression (should be required)")]
            public string Arg { get; set; } = null!;

            [CliArgument(Description = "Argument with non-nullable reference type with default value (should not be required)")]
            public string ArgDefault { get; set; } = "test";

            public void Run(CliContext context)
            {
                context.ShowValues();
            }
        }


        [CliCommand]
        public class RequiredCommand
        {
            [CliOption(
                Description = "Option with required keyword with no default value (should be required)",
                AllowedValues = new[] { "Big", "Small" }
            )]
            public required string Opt { get; set; }

            [CliOption(
                Description = "Option with required keyword with default value (should not be required)",
                AllowedValues = new[] { "Big", "Small" }
            )]
            public required string OptDefault { get; set; } = "Big";

            [CliArgument(Description = "Argument with required keyword with no default value (should be required)")]
            public required string Arg { get; set; }

            [CliArgument(Description = "Argument with required keyword with default value (should not be required)")]
            public required string ArgDefault { get; set; } = "test";

            public void Run(CliContext context)
            {
                context.ShowValues();
            }
        }
    }
}
