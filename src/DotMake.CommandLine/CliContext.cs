using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using DotMake.CommandLine.Help;
using DotMake.CommandLine.Util;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Supports command invocation by providing access to parse results and other services.
    /// </summary>
    public class CliContext
    {
        private readonly ParseResult parseResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliContext" /> class.
        /// </summary>
        /// <param name="bindingContext">The context used during binding of commands.</param>
        /// <param name="parseResult">The result providing details about the parse operation.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public CliContext(CliBindingContext bindingContext, ParseResult parseResult, CancellationToken cancellationToken = default)
        {
            Result = new CliResult(bindingContext, parseResult);
            this.parseResult = parseResult;

            Output = parseResult.Configuration.Output;
            Error = parseResult.Configuration.Error;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the result providing details about the parse operation and methods for binding.
        /// </summary>
        public CliResult Result { get; }

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
        /// Gets a value indicating whether current command is specified without any arguments or options.
        /// <para>
        /// Note that arguments and options should be optional, if they are required (no default values),
        /// then handler will not run and missing error message will be shown.
        /// </para>
        /// </summary>
        /// <returns><see langword="true"/> if current command has no arguments or options, <see langword="false"/> if not.</returns>
        public bool IsEmptyCommand()
        {
            return (parseResult.CommandResult.Tokens.Count == 0);
        }

        /// <summary>
        /// Gets a value indicating whether current command is specified without any commands, directives, options or arguments.
        /// <para>
        /// Note that arguments and options should be optional, if they are required (no default values),
        /// then handler will not run and missing error message will be shown.
        /// </para>
        /// </summary>
        /// <returns><see langword="true"/> if current command has no arguments or options, <see langword="false"/> if not.</returns>
        public bool IsEmpty()
        {
            return (parseResult.Tokens.Count == 0);
        }

        /// <summary>
        /// Shows help for current command.
        /// </summary>
        public void ShowHelp()
        {
            var helpOption = GetHelpOptionOrDefault();

            var action = (SynchronousCommandLineAction)helpOption.Action;

            action?.Invoke(parseResult);
        }

        /// <summary>
        /// Shows parsed values for current command and its arguments and options. Useful for testing a command.
        /// </summary>
        public void ShowValues()
        {
            var output = parseResult.Configuration.Output;

            var command = parseResult.CommandResult.Command;
            var isRoot = (command.Parents.FirstOrDefault() == null);

            output.WriteLine($"Command = \"{command.Name}\" [{(isRoot ? "Root command" : "Sub-command")}]");

            foreach (var symbolResult in parseResult.CommandResult.Children)
            {
                if (symbolResult is ArgumentResult argumentResult)
                {
                    var value = CliCommandBuilder.GetValueForArgument(parseResult, argumentResult.Argument);
                    output.WriteLine($"Argument '{argumentResult.Argument.Name}' = {StringExtensions.FormatValue(value)}");
                }
                else if (symbolResult is OptionResult optionResult)
                {
                    var value = CliCommandBuilder.GetValueForOption(parseResult, optionResult.Option);
                    output.WriteLine($"Option '{optionResult.Option.Name}' = {StringExtensions.FormatValue(value)}");
                }
            }
        }

        /// <summary>
        /// Shows hierarchy for all commands. It will start from the root command and show a tree. Useful for testing a command.
        /// </summary>
        /// <param name="showLevel">Whether to show level labels next to the tree nodes.</param>
        public void ShowHierarchy(bool showLevel = false)
        {
            var theme = GetThemeOrDefault();
            var rootCommand = parseResult.Configuration.RootCommand;

            foreach (var command in GetParentTree(rootCommand))
            {
                var parent = command.Parents.FirstOrDefault() as Command;
                var isLast = (parent?.Subcommands[parent.Subcommands.Count - 1] == command);
                var isRoot = (command == rootCommand);


                var indent = " ";
                var level = isRoot ? 0 : 1;
                var current = parent;
                while (current != null && current != rootCommand)
                {
                    var currentParent = current.Parents.FirstOrDefault() as Command;
                    var currentHasChildren = current.Subcommands.Count > 0;
                    var currentIsLast = (currentParent?.Subcommands[currentParent.Subcommands.Count - 1] == current);

                    indent += (currentHasChildren && !currentIsLast ? "│ " : "  ");
                    level++;

                    current = currentParent;
                }

                var charArray = indent.ToCharArray();
                Array.Reverse(charArray); //we reverse because we loop parents above reversely, so tree symbols are reverse
                indent = new string(charArray);

                Console.Write(indent + (isRoot ? "" : isLast ? "└╴" : "├╴"));

                /*
                var level = command
                    .RecurseWhileNotNull(c => c.Parents.OfType<Command>().FirstOrDefault())
                    .Count() - 1;
                */

                ConsoleExtensions.SetColor(theme.FirstColumnColor, theme.DefaultColor);
                Console.Write($@"{command.Name}");
                ConsoleExtensions.SetColor(theme.DefaultColor);

                if (showLevel)
                {
                    ConsoleExtensions.SetColor(theme.SecondColumnColor, theme.DefaultColor);
                    Console.Write($@" (level {level})");
                    ConsoleExtensions.SetColor(theme.DefaultColor);
                }
                
                Console.WriteLine();
            }
        }

        private IEnumerable<Command> GetParentTree(Command rootCommand)
        {
            // Use Stack (depth-first) for correct order here
            // https://stackoverflow.com/questions/5804844/implementing-depth-first-search-into-c-sharp-using-list-and-stack
            var stack = new Stack<Command>();
            stack.Push(rootCommand);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                yield return current;

                // Reverse() is required for the left-to-right order in depth-first
                foreach (var child in current.Subcommands.Reverse())
                {
                    stack.Push(child);
                }
            }
        }

        private HelpOption GetHelpOptionOrDefault()
        {
            var option =
                parseResult.RootCommandResult.Command.Options.FirstOrDefault(option => option is HelpOption)
                ?? parseResult.CommandResult.Command.Options.FirstOrDefault(option => option is HelpOption)
                ?? new HelpOption
                {
                    Action = new CustomHelpAction
                    {
                        Builder = new CliHelpBuilder(CliTheme.Default, Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth)
                    }
                };

            return (HelpOption)option;
        }

        private CliTheme GetThemeOrDefault()
        {
            return GetHelpOptionOrDefault().Action is CustomHelpAction helpAction
                   && helpAction.Builder is CliHelpBuilder cliHelpBuilder
                ? cliHelpBuilder.Theme
                : CliTheme.Default;
        }
    }
}
