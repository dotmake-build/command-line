using System;
using System.Collections;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="InvocationContext" />.
    /// </summary>
    public static class InvocationContextExtensions
    {
        /// <summary>
        /// Shows help for current command.
        /// </summary>
        /// <param name="context">The invocation context.</param>
        public static void ShowHelp(this InvocationContext context)
        {
            using (var output = context.Console.Out.CreateTextWriter())
            {
                var helpContext = new HelpContext(context.HelpBuilder, context.ParseResult.CommandResult.Command, output, context.ParseResult);
                context.HelpBuilder.Write(helpContext);
            }
        }

        /// <summary>
        /// Gets a value indicating whether current command is specified without any arguments or options.
        /// </summary>
        /// <param name="context">The invocation context.</param>
        /// <returns><see langword="true"/> if current command has no arguments or options, <see langword="false"/> if not.</returns>
        public static bool IsEmptyCommand(this InvocationContext context)
        {
            return (context.ParseResult.CommandResult.Tokens.Count == 0);
        }

        /// <summary>
        /// Shows parsed values for current command and its arguments and options. Useful for testing a command.
        /// </summary>
        /// <param name="context">The invocation context.</param>
        public static void ShowValues(this InvocationContext context)
        {
            using (var output = context.Console.Out.CreateTextWriter())
            {
                var command = context.ParseResult.CommandResult.Command;
                var isRoot = (command.Parents.FirstOrDefault() == null);

                output.WriteLine($"Command = \"{command.Name}\" [{(isRoot ? "Root command" : "Sub-command")}]");

                foreach (var symbolResult in context.ParseResult.CommandResult.Children)
                {
                    if (symbolResult.Symbol is Argument argument)
                    {
                        var value = CliCommandBuilder.GetValueForArgument(context.ParseResult, argument);
                        output.WriteLine($"Argument '{argument.Name}' = {FormatValue(value)}");
                    }
                    else if (symbolResult.Symbol is Option option)
                    {
                        var value = CliCommandBuilder.GetValueForOption(context.ParseResult, option);
                        output.WriteLine($"Option '{option.Name}' = {FormatValue(value)}");
                    }
                }
            }
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "null";

            if (value is string)
                return value.ToString().ToLiteral();

            if (value is char)
                return value.ToString().ToLiteral('\'');

            if (value is bool)
                return value.ToString().ToLowerInvariant();

            if (value is IFormattable)
                return value.ToString();

            if (value is IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToArray();

                return "[" + string.Join(", ", items.Select(FormatValue)) + "]";
            }

            if (value.ToString() != value.GetType().ToString())
                return value.ToString();

            return "object";
        }
    }
}
