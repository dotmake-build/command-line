using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Formats output to be shown to users to describe how to use a command line tool.
    /// <para>
    /// <see cref="HelpBuilder"/> is weirdly designed, i.e. it's hard to derive from that class due to static methods.
    /// <see cref="CliHelpBuilder"/> solves this problem by providing overridable methods, and it also adds color support.
    /// </para>
    /// </summary>
    public class CliHelpBuilder : HelpBuilder
    {
        private const string Indent = "  ";
        private readonly IConsole console;
        private static readonly MethodInfo IsGlobalGetter = typeof(Option)
            .GetProperty("IsGlobal", BindingFlags.NonPublic | BindingFlags.Instance)?.GetMethod;
        private static readonly PropertyInfo ArgumentProperty =
            typeof(Option).GetProperty("Argument", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Initializes a new instance of the <see cref="CliHelpBuilder" /> class.
        /// </summary>
        /// <param name="localizationResources">Resources used to localize the help output.</param>
        /// <param name="maxWidth">The maximum width in characters after which help output is wrapped.</param>
        /// <param name="console">The console to which output should be written during the current invocation.</param>
        public CliHelpBuilder(LocalizationResources localizationResources, int maxWidth = int.MaxValue, IConsole console = null)
            : base(localizationResources, maxWidth)
        {
            //CustomizeLayout(GetLayout);
            this.console = console;
        }

        /// <summary>
        /// Gets the default sections to be written for command line help.
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        /// <returns>An enumerable whose elements are the <see cref="Func{HelpContext, Boolean}"/> instances which writes a section.</returns>
        public virtual IEnumerable<Func<HelpContext, bool>> GetLayout(HelpContext helpContext)
        {
            yield return WriteSynopsisSection;
            yield return WriteCommandUsageSection;
            yield return WriteCommandArgumentsSection;
            yield return WriteCommandOptionsSection;
            yield return WriteSubcommandsSection;
            yield return WriteAdditionalArgumentsSection;
        }

        /// <summary>
        /// Writes help output for the specified command.
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        public override void Write(HelpContext helpContext)
        {
            if (helpContext is null)
            {
                throw new ArgumentNullException(nameof(helpContext));
            }

            if (helpContext.Command.IsHidden || helpContext.ParseResult.Errors.Count > 0)
            {
                return;
            }

            var writeSections = GetLayout(helpContext).ToArray();
            foreach (var writeSection in writeSections)
            {
                if (writeSection(helpContext))
                    helpContext.Output.WriteLine();
            }
        }

        /// <summary>
        /// Writes a help section describing a command's synopsis.
        /// Similar to:
        /// <code language="console">
        /// DotMake Command-Line TestApp v1.6.0
        /// Copyright © 2023-2024 DotMake
        ///
        /// A root cli command with nested children
        /// </code>
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        /// <returns><see langword="true"/> if section was written, <see langword="false"/> if section was skipped.</returns>
        public virtual bool WriteSynopsisSection(HelpContext helpContext)
        {
            helpContext.Output.Write(ExecutableInfo.Product);
            if (!string.IsNullOrWhiteSpace(ExecutableInfo.Version))
                helpContext.Output.Write(" v{0}", ExecutableInfo.Version);
            helpContext.Output.WriteLine();
            if (!string.IsNullOrWhiteSpace(ExecutableInfo.Copyright))
                helpContext.Output.WriteLine(ExecutableInfo.Copyright);

            var isRoot = (helpContext.Command.Parents.FirstOrDefault() == null);
            var name = isRoot ? string.Empty : helpContext.Command.Name;
            var description = helpContext.Command.Description
                              ?? (isRoot ? ExecutableInfo.Description : string.Empty);
            const string separator = ": ";
            
            var hasName = !string.IsNullOrWhiteSpace(name);
            var hasDescription = !string.IsNullOrWhiteSpace(description);

            if (hasName || hasDescription)
            {
                helpContext.Output.WriteLine();

                if (hasName)
                {
                    console.SetForegroundColor(ConsoleColor.White);
                    if (hasDescription)
                        helpContext.Output.Write(name);
                    else
                        helpContext.Output.WriteLine(name);
                    console.ResetForegroundColor();
                }

                if (hasDescription)
                {
                    if (hasName)
                        helpContext.Output.Write(separator);

                    var maxWidth = hasName
                        ? MaxWidth - name.Length - separator.Length
                        : MaxWidth;

                    foreach (var part in WrapText(description, maxWidth))
                        helpContext.Output.WriteLine(part);
                }
            }

            /*
            WriteHeading(
                LocalizationResources.HelpDescriptionTitle(),
                helpContext.Command.Description,
                helpContext.Output
            );*/

            return true;
        }

        /// <summary>
        /// Writes a help section describing a command's usage.
        /// Similar to:
        /// <code language="console">
        /// Usage:
        ///   TestApp &lt;argument-1&gt; [command] [options]
        /// </code>
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        /// <returns><see langword="true"/> if section was written, <see langword="false"/> if section was skipped.</returns>
        public virtual bool WriteCommandUsageSection(HelpContext helpContext)
        {
            var usage = string.Join(" ", GetUsageParts(helpContext.Command).Where(x => !string.IsNullOrWhiteSpace(x)));

            WriteHeading(
                LocalizationResources.HelpUsageTitle(),
                usage,
                helpContext.Output
            );

            return true;
        }

        /// <summary>
        /// Writes a help section describing a command's arguments.
        /// Similar to:
        /// <code language="console">
        /// Arguments:
        ///   &lt;argument-1&gt;  Description for Argument1 [required]
        /// </code>
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        /// <returns><see langword="true"/> if section was written, <see langword="false"/> if section was skipped.</returns>
        public virtual bool WriteCommandArgumentsSection(HelpContext helpContext)
        {
            var commandArguments = GetCommandArgumentRows(helpContext.Command, helpContext).ToArray();

            if (commandArguments.Length <= 0)
                return false;

            WriteHeading(
                LocalizationResources.HelpArgumentsTitle(),
                null,
                helpContext.Output
            );

            WriteColumns(commandArguments, helpContext);

            return true;
        }

        /// <summary>
        /// Writes a help section describing a command's options.
        /// Similar to:
        /// <code language="console">
        /// Options:
        ///   -o, --option-1 &lt;option-1&gt;  Description for Option1 [default: DefaultForOption1]
        ///   -v, --version Show version information
        ///   -?, -h, --help Show help and usage information
        /// </code>
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        /// <returns><see langword="true"/> if section was written, <see langword="false"/> if section was skipped.</returns>
        public virtual bool WriteCommandOptionsSection(HelpContext helpContext)
        {
            // by making this logic more complex, we were able to get some nice perf wins elsewhere
            var options = new List<TwoColumnHelpRow>();
            var uniqueOptions = new HashSet<Option>();
            foreach (var option in helpContext.Command.Options)
            {
                if (!option.IsHidden && uniqueOptions.Add(option))
                {
                    options.Add(GetTwoColumnRow(option, helpContext));
                }
            }

            var current = helpContext.Command;
            while (current != null)
            {
                Command parentCommand = null;
                foreach (var parent in current.Parents)
                {
                    if ((parentCommand = parent as Command) != null)
                    {
                        foreach (var option in parentCommand.Options)
                        {
                            // global help aliases may be duplicated, we just ignore them
                            if ((IsGlobalGetter?.Invoke(option, null) as bool?) == true && !option.IsHidden && uniqueOptions.Add(option))
                            {
                                options.Add(GetTwoColumnRow(option, helpContext));
                            }
                        }

                        break;
                    }
                }

                current = parentCommand;
            }

            if (options.Count <= 0)
                return false;

            WriteHeading(
                LocalizationResources.HelpOptionsTitle(),
                null,
                helpContext.Output
            );

            WriteColumns(options, helpContext);

            return true;
        }

        /// <summary>
        /// Writes a help section describing a command's subcommands.
        /// Similar to:
        /// <code language="console">
        /// Commands:
        ///   level-1  A nested level 1 sub-command
        /// </code>
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        /// <returns><see langword="true"/> if section was written, <see langword="false"/> if section was skipped.</returns>
        public virtual bool WriteSubcommandsSection(HelpContext helpContext)
        {
            var subcommands = helpContext.Command.Subcommands
                .Where(x => !x.IsHidden)
                .Select(x => GetTwoColumnRow(x, helpContext)).ToArray();

            if (subcommands.Length <= 0)
                return false;

            WriteHeading(LocalizationResources.HelpCommandsTitle(), null, helpContext.Output);

            WriteColumns(subcommands, helpContext);

            return true;
        }

        /// <summary>
        /// Writes a help section describing a command's additional arguments, typically shown only when <see cref="Command.TreatUnmatchedTokensAsErrors"/> is set to <see langword="true"/>.
        /// </summary>
        /// <param name="helpContext">The help context.</param>
        /// <returns><see langword="true"/> if section was written, <see langword="false"/> if section was skipped.</returns>
        public virtual bool WriteAdditionalArgumentsSection(HelpContext helpContext)
        {
            if (helpContext.Command.TreatUnmatchedTokensAsErrors)
                return false;

            WriteHeading(
                LocalizationResources.HelpAdditionalArgumentsTitle(),
                LocalizationResources.HelpAdditionalArgumentsDescription(),
                helpContext.Output
            );

            return true;
        }

        private void WriteHeading(string heading, string description, TextWriter writer)
        {
            if (!string.IsNullOrWhiteSpace(heading))
            {
                writer.WriteLine(heading);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                var maxWidth = MaxWidth - Indent.Length;
                foreach (var part in WrapText(description, maxWidth))
                {
                    writer.Write(Indent);
                    writer.WriteLine(part);
                }
            }
        }

        /// <summary>
        /// Writes the specified help rows, aligning output in columns.
        /// </summary>
        /// <param name="items">The help items to write out in columns.</param>
        /// <param name="helpContext">The help context.</param>
        public new void WriteColumns(IReadOnlyList<TwoColumnHelpRow> items, HelpContext helpContext)
        {
            if (items.Count == 0)
            {
                return;
            }

            var windowWidth = MaxWidth;

            var firstColumnWidth = items.Select(x => x.FirstColumnText.Length).Max();
            var secondColumnWidth = items.Select(x => x.SecondColumnText.Length).Max();

            if (firstColumnWidth + secondColumnWidth + Indent.Length + Indent.Length > windowWidth)
            {
                var firstColumnMaxWidth = windowWidth / 2 - Indent.Length;
                if (firstColumnWidth > firstColumnMaxWidth)
                {
                    firstColumnWidth = items.SelectMany(x => WrapText(x.FirstColumnText, firstColumnMaxWidth).Select(x2 => x2.Length)).Max();
                }
                secondColumnWidth = windowWidth - firstColumnWidth - Indent.Length - Indent.Length;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var helpItem = items[i];
                var firstColumnParts = WrapText(helpItem.FirstColumnText, firstColumnWidth);
                var secondColumnParts = WrapText(helpItem.SecondColumnText, secondColumnWidth);

                foreach (var (first, second) in ZipWithEmpty(firstColumnParts, secondColumnParts))
                {
                    console.SetForegroundColor(ConsoleColor.White);
                    helpContext.Output.Write($"{Indent}{first}");
                    console.ResetForegroundColor();

                    if (!string.IsNullOrWhiteSpace(second))
                    {
                        var padSize = firstColumnWidth - first.Length;
                        var padding = "";
                        if (padSize > 0)
                        {
                            padding = new string(' ', padSize);
                        }

                        helpContext.Output.Write($"{padding}{Indent}{second}");
                    }

                    helpContext.Output.WriteLine();
                }
            }
        }

        private IEnumerable<string> GetUsageParts(Command command)
        {
            var displayOptionTitle = false;

            var parentCommands =
                command
                    .RecurseWhileNotNull(c => c.Parents.OfType<Command>().FirstOrDefault())
                    .Reverse();

            foreach (var parentCommand in parentCommands)
            {
                if (!displayOptionTitle)
                {
                    displayOptionTitle = parentCommand.Options.Any(x => (IsGlobalGetter?.Invoke(x, null) as bool?) == true && !x.IsHidden);
                }

                if (parentCommand.Parents.FirstOrDefault() == null)
                    yield return ExecutableInfo.ExecutableName;
                else
                    yield return parentCommand.Name;

                yield return FormatArgumentUsage(parentCommand.Arguments);
            }

            var hasCommandWithHelp = command.Subcommands.Any(x => !x.IsHidden);

            if (hasCommandWithHelp)
            {
                yield return LocalizationResources.HelpUsageCommand();
            }

            displayOptionTitle = displayOptionTitle || command.Options.Any(x => !x.IsHidden);

            if (displayOptionTitle)
            {
                yield return LocalizationResources.HelpUsageOptions();
            }

            if (!command.TreatUnmatchedTokensAsErrors)
            {
                yield return LocalizationResources.HelpUsageAdditionalArguments();
            }
        }

        private static IEnumerable<string> WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            //First handle existing new lines
            var parts = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var part in parts)
            {
                if (part.Length <= maxWidth)
                {
                    yield return part;
                }
                else
                {
                    //Long item, wrap it based on the width
                    for (var i = 0; i < part.Length;)
                    {
                        if (part.Length - i < maxWidth)
                        {
                            yield return part.Substring(i);
                            break;
                        }
                        else
                        {
                            var length = -1;
                            for (var j = 0; j + i < part.Length && j < maxWidth; j++)
                            {
                                if (char.IsWhiteSpace(part[i + j]))
                                {
                                    length = j + 1;
                                }
                            }
                            if (length == -1)
                            {
                                length = maxWidth;
                            }
                            yield return part.Substring(i, length);

                            i += length;
                        }
                    }
                }
            }
        }

        private static bool IsMultiParented(Argument a) =>
            a.Parents.Take(2).Count() == 2;

        private static bool IsOptional(Argument argument) =>
            IsMultiParented(argument) ||
            argument.Arity.MinimumNumberOfValues == 0;

        private static IEnumerable<(string, string)> ZipWithEmpty(IEnumerable<string> first, IEnumerable<string> second)
        {
            using (var enum1 = first.GetEnumerator())
            using (var enum2 = second.GetEnumerator())
            {
                bool hasFirst, hasSecond;
                while ((hasFirst = enum1.MoveNext()) | (hasSecond = enum2.MoveNext()))
                {
                    yield return (hasFirst ? enum1.Current : "", hasSecond ? enum2.Current : "");
                }
            }
        }

        private static string FormatArgumentUsage(IReadOnlyList<Argument> arguments)
        {
            var sb = new StringBuilder();

            var end = default(Stack<char>);

            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                if (argument.IsHidden)
                {
                    continue;
                }

                var arityIndicator =
                    argument.Arity.MaximumNumberOfValues > 1
                        ? "..."
                        : "";

                var isOptional = IsOptional(argument);

                if (isOptional)
                {
                    sb.Append($"[<{argument.Name}>{arityIndicator}");
                    (end = end ?? new Stack<char>()).Push(']');
                }
                else
                {
                    sb.Append($"<{argument.Name}>{arityIndicator}");
                }

                sb.Append(' ');
            }

            if (sb.Length > 0)
            {
                sb.Length--;

                if (end != null)
                {
                    while (end.Count > 0)
                    {
                        sb.Append(end.Pop());
                    }
                }
            }

            return sb.ToString();
        }

        private IEnumerable<TwoColumnHelpRow> GetCommandArgumentRows(Command command, HelpContext helpContext) =>
            command
                .RecurseWhileNotNull(c => c.Parents.OfType<Command>().FirstOrDefault())
                .Reverse()
                .SelectMany(cmd => cmd.Arguments.Where(a => !a.IsHidden))
                .Select(a => GetTwoColumnRow(a, helpContext))
                .Distinct();

        /// <summary>
        /// Gets a help item for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <param name="context">The help context.</param>
        /// <returns>Two column help row.</returns>
        public new TwoColumnHelpRow GetTwoColumnRow(
            Symbol symbol,
            HelpContext context)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            if (symbol is IdentifierSymbol identifierSymbol)
            {
                return GetIdentifierSymbolRow();
            }
            else if (symbol is Argument argument)
            {
                return GetCommandArgumentRow(argument);
            }
            else
            {
                throw new NotSupportedException($"Symbol type {symbol.GetType()} is not supported.");
            }

            TwoColumnHelpRow GetIdentifierSymbolRow()
            {
                var firstColumnText = GetIdentifierSymbolUsageLabel(identifierSymbol);

                var symbolDescription = GetIdentifierSymbolDescription(identifierSymbol, context);

                //in case symbol description is customized, do not output default value
                //default value output is not customizable for identifier symbols
                var defaultValueDescription =  GetSymbolDefaultValue(identifierSymbol);

                var secondColumnText = $"{symbolDescription} {defaultValueDescription}".Trim();

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }

            TwoColumnHelpRow GetCommandArgumentRow(Argument argument)
            {
                var firstColumnText = GetArgumentUsageLabel(argument);

                var argumentDescription = GetArgumentDescription(argument, context);

                var defaultValueDescription =
                    argument.HasDefaultValue
                        ? GetArgumentDefaultValue(argument, true)
                        : "";

                if (defaultValueDescription.Length > 0)
                    defaultValueDescription = $"[{defaultValueDescription}]";

                var secondColumnText = $"{argumentDescription} {defaultValueDescription}".Trim();

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }

            string GetSymbolDefaultValue(IdentifierSymbol symbol2)
            {
                IEnumerable<Argument> arguments = GetArguments(symbol2);
                var defaultArguments = arguments.Where(x => !x.IsHidden && x.HasDefaultValue).ToArray();

                if (defaultArguments.Length == 0) return "";

                var isSingleArgument = defaultArguments.Length == 1;
                var argumentDefaultValues = defaultArguments
                    .Select(argument => GetArgumentDefaultValue(argument, isSingleArgument));
                var joined = string.Join(", ", argumentDefaultValues);
                return joined.Length > 0 ? $"[{joined}]" : string.Empty;
            }
        }

        private string GetArgumentDefaultValue(
            Argument argument,
            bool displayArgumentName)
        {
            var label = displayArgumentName
                              ? LocalizationResources.HelpArgumentDefaultValueLabel()
                              : argument.Name;

            var displayedDefaultValue = Default.GetArgumentDefaultValue(argument);

            if (string.IsNullOrWhiteSpace(displayedDefaultValue))
            {
                return "";
            }

            return $"{label}: {displayedDefaultValue}";
        }

        private static IReadOnlyList<Argument> GetArguments(Symbol symbol)
        {
            switch (symbol)
            {
                case Option option:
                    return new[]
                    {
                        (Argument)ArgumentProperty.GetValue(option)
                    };
                case Command command:
                    return command.Arguments;
                case Argument argument:
                    return new[]
                    {
                        argument
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets the usage label for the specified symbol (typically used as the first column text in help output).
        /// </summary>
        /// <param name="symbol">The symbol to get a help item for.</param>
        /// <returns>Text to display.</returns>
        private static string GetIdentifierSymbolUsageLabel(IdentifierSymbol symbol)
        {
            var aliases = symbol.Aliases
                .Select(r => r.SplitPrefix())
                .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase)
                .GroupBy(t => t.Alias)
                .Select(t => t.First())
                .Select(t => $"{t.Prefix}{t.Alias}");

            var firstColumnText = string.Join(", ", aliases);

            if (symbol is Command)
                return firstColumnText;

            foreach (var argument in GetArguments(symbol))
            {
                if (!argument.IsHidden)
                {
                    var argumentFirstColumnText = Default.GetArgumentUsageLabel(argument);

                    if (!string.IsNullOrWhiteSpace(argumentFirstColumnText))
                    {
                        firstColumnText += $" {argumentFirstColumnText}";
                    }
                }
            }

            return firstColumnText;
        }

        /// <summary>
        /// Gets the description for the specified symbol (typically the used as the second column in help text).
        /// </summary>
        /// <param name="symbol">The symbol to get the description for.</param>
        /// <param name="context">The help context, used for localization purposes.</param>
        private static string GetIdentifierSymbolDescription(IdentifierSymbol symbol, HelpContext context)
        {
            var secondColumnText = symbol.Description ?? string.Empty;

            if (symbol is Option option && option.IsRequired)
            {
                if (secondColumnText.Length > 0)
                    secondColumnText += " ";

                secondColumnText += GetRequiredLabel(context);
            }

            return secondColumnText;
        }

        /// <summary>
        /// Gets the usage title for an argument (for example: <c>&lt;value&gt;</c>, typically used in the first column text in the arguments usage section, or within the synopsis.
        /// </summary>
        private static string GetArgumentUsageLabel(Argument argument)
        {
            if (argument.ValueType == typeof(bool) ||
                argument.ValueType == typeof(bool?))
            {
                if (argument.Parents.FirstOrDefault() is Command)
                {
                    return $"<{argument.Name}>";
                }
                else
                {
                    return "";
                }
            }

            string firstColumn;
            var completions =
                argument.GetCompletions()
                .Select(item => item.Label)
                .ToArray();

            var arg = argument;
            var helpName = arg.HelpName ?? string.Empty;

            if (!string.IsNullOrEmpty(helpName))
            {
                firstColumn = helpName;
            }
            else if (completions.Length > 0)
            {
                firstColumn = string.Join("|", completions);
            }
            else
            {
                firstColumn = argument.Name;
            }

            if (!string.IsNullOrWhiteSpace(firstColumn))
            {
                return $"<{firstColumn}>";
            }
            return firstColumn;
        }

        /// <summary>
        /// Gets the description for an argument (typically used in the second column text in the arguments section).
        /// </summary>
        private static string GetArgumentDescription(Argument argument, HelpContext context)
        {
            var secondColumnText = argument.Description ?? string.Empty;

            if (!argument.HasDefaultValue)
            {
                if (secondColumnText.Length > 0)
                    secondColumnText += " ";

                secondColumnText += GetRequiredLabel(context);
            }

            return secondColumnText;
        }

        private static string GetRequiredLabel(HelpContext context)
        {
            var label = context.HelpBuilder.LocalizationResources.HelpOptionsRequiredLabel()
                .TrimStart('(').TrimEnd(')').ToLowerInvariant();

            return $"[{label}]";
        }
    }
}
