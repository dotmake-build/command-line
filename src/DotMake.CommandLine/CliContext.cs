using System;
using System.Collections;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Supports command invocation by providing access to parse results and other services.
    /// </summary>
    public class CliContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CliContext" /> class.
        /// </summary>
        /// <param name="parseResult">The parse result for the current invocation.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public CliContext(ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            ParseResult = parseResult;
            Output = parseResult.Configuration.Output;
            Error = parseResult.Configuration.Error;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the parse result for the current invocation.
        /// </summary>
        public ParseResult ParseResult { get; }

        /// <summary>
        /// Gets the standard output, to which non-error information should be written during the current invocation.
        /// By default, it's set to <see cref="P:System.Console.Out" />, it's changed via <see cref="CliSettings.Output"/>.
        /// </summary>
        public TextWriter Output { get; }

        /// <summary>
        /// Gets the standard error, to which error information should be written during the current invocation.
        /// By default, it's set to <see cref="P:System.Console.Error" />, it's changed via <see cref="CliSettings.Error"/>.
        /// </summary>
        public TextWriter Error { get; }

        /// <summary>
        /// Gets the token to implement cancellation handling. Available for async command handlers.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Shows help for current command.
        /// </summary>
        public  void ShowHelp()
        {
            var helpOption =
                ParseResult.RootCommandResult.Command.Options.FirstOrDefault(option => option is HelpOption)
                ?? ParseResult.CommandResult.Command.Options.FirstOrDefault(option => option is HelpOption)
                ?? new HelpOption
                {
                    Action = new HelpAction
                    {
                        Builder = new CliHelpBuilder(CliTheme.Default, Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth)
                    }
                };

            var action = (SynchronousCommandLineAction)helpOption.Action;

            action?.Invoke(ParseResult);
        }

        /// <summary>
        /// Gets a value indicating whether current command is specified without any arguments or options.
        /// </summary>
        /// <returns><see langword="true"/> if current command has no arguments or options, <see langword="false"/> if not.</returns>
        public bool IsEmptyCommand()
        {
            return (ParseResult.CommandResult.Tokens.Count == 0);
        }

        /// <summary>
        /// Shows parsed values for current command and its arguments and options. Useful for testing a command.
        /// </summary>
        public void ShowValues()
        {
            var output = ParseResult.Configuration.Output;

            var command = ParseResult.CommandResult.Command;
            var isRoot = (command.Parents.FirstOrDefault() == null);

            output.WriteLine($"Command = \"{command.Name}\" [{(isRoot ? "Root command" : "Sub-command")}]");

            foreach (var symbolResult in ParseResult.CommandResult.Children)
            {
                if (symbolResult is ArgumentResult argumentResult)
                {
                    var value = CliCommandBuilder.GetValueForArgument(ParseResult, argumentResult.Argument);
                    output.WriteLine($"Argument '{argumentResult.Argument.Name}' = {FormatValue(value)}");
                }
                else if (symbolResult is OptionResult optionResult)
                {
                    var value = CliCommandBuilder.GetValueForOption(ParseResult, optionResult.Option);
                    output.WriteLine($"Option '{optionResult.Option.Name}' = {FormatValue(value)}");
                }
            }
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "null";

            if (value is string stringValue)
                return stringValue.ToLiteral();

            if (value is char charValue)
                return charValue.ToString().ToLiteral('\'');

            if (value is bool boolValue)
                return boolValue.ToString().ToLowerInvariant();

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
