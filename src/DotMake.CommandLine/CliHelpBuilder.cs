using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using DotMake.CommandLine.Help;
using DotMake.CommandLine.Util;

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
        private readonly CliTheme theme;
        private Func<HelpContext, IEnumerable<Func<HelpContext, bool>>> customGetLayout;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliHelpBuilder" /> class.
        /// </summary>
        /// <param name="theme">The theme to use for help output</param>
        /// <param name="maxWidth">The maximum width in characters after which help output is wrapped.</param>
        public CliHelpBuilder(CliTheme theme, int maxWidth = int.MaxValue)
            : base(maxWidth)
        {
            this.theme = theme;
        }

        /// <summary>
        /// Gets the theme used by this <see cref="CliHelpBuilder"/>.
        /// </summary>
        public CliTheme Theme => theme;

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

            if (helpContext.Command.Hidden || helpContext.ParseResult.Errors.Count > 0)
            {
                return;
            }

            var getLayout = customGetLayout ?? GetLayout;     
            var writeSections = getLayout(helpContext).ToArray();
            foreach (var writeSection in writeSections)
            {
                if (writeSection(helpContext))
                    helpContext.Output.WriteLine();
            }
        }
        /// <summary>
        /// Customizes the layout of the help output.
        /// </summary>
        /// <param name="getLayout">A delegate that returns the layout sections.</param>
        public new void CustomizeLayout(Func<HelpContext, IEnumerable<Func<HelpContext, bool>>> getLayout)
        {
            customGetLayout = getLayout ?? throw new ArgumentNullException(nameof(getLayout));
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
            ConsoleExtensions.SetColor(theme.SynopsisColor, theme.DefaultColor);
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
                    ConsoleExtensions.SetColor(theme.FirstColumnColor, theme.DefaultColor);
                    helpContext.Output.Write(name);
                    ConsoleExtensions.SetColor(theme.SynopsisColor, theme.DefaultColor);
                    if (!hasDescription)
                        helpContext.Output.WriteLine();
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

            ConsoleExtensions.SetColor(theme.DefaultColor);

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
            WriteHeading(
                LocalizationResources.HelpUsageTitle(),
                GetUsage(helpContext.Command),
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
        ///   -v, --version              Show version information
        ///   -?, -h, --help             Show help and usage information
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
                if (!option.Hidden && uniqueOptions.Add(option))
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
                            if (option.Recursive && !option.Hidden && uniqueOptions.Add(option))
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
                .Where(x => !x.Hidden)
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
                heading = heading.ToCase(theme.HeadingCasing, keepSpaces: true);
                if (theme.HeadingNoColon)
                    heading = heading.TrimEnd(':');
                ConsoleExtensions.SetColor(theme.HeadingColor, theme.DefaultColor);
                writer.WriteLine(heading);
                ConsoleExtensions.SetColor(theme.DefaultColor);
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                int maxWidth = MaxWidth - Indent.Length;
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
        /// <param name="context">The help context.</param>
        public new void WriteColumns(IReadOnlyList<TwoColumnHelpRow> items, HelpContext context)
        {
            if (items.Count == 0)
            {
                return;
            }

            int windowWidth = MaxWidth;

            int firstColumnWidth = items.Select(x => x.FirstColumnText.Length).Max();
            int secondColumnWidth = items.Select(x => x.SecondColumnText.Length).Max();

            if (firstColumnWidth + secondColumnWidth + Indent.Length + Indent.Length > windowWidth)
            {
                int firstColumnMaxWidth = windowWidth / 2 - Indent.Length;
                if (firstColumnWidth > firstColumnMaxWidth)
                {
                    firstColumnWidth = items.SelectMany(x => WrapText(x.FirstColumnText, firstColumnMaxWidth).Select(x => x.Length)).Max();
                }
                secondColumnWidth = windowWidth - firstColumnWidth - Indent.Length - Indent.Length;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var helpItem = items[i];
                IEnumerable<string> firstColumnParts = WrapText(helpItem.FirstColumnText, firstColumnWidth);
                IEnumerable<string> secondColumnParts = WrapText(helpItem.SecondColumnText, secondColumnWidth);

                foreach (var (first, second) in ZipWithEmpty(firstColumnParts, secondColumnParts))
                {
                    /*MODIFY*/
                    ConsoleExtensions.SetColor(theme.FirstColumnColor, theme.DefaultColor);
                    context.Output.Write($"{Indent}{first}");
                    ConsoleExtensions.SetColor(theme.DefaultColor);
                    /*MODIFY*/

                    if (!string.IsNullOrWhiteSpace(second))
                    {
                        int padSize = firstColumnWidth - first.Length;
                        string padding = "";
                        if (padSize > 0)
                        {
                            padding = new string(' ', padSize);
                        }

                        ConsoleExtensions.SetColor(theme.SecondColumnColor, theme.DefaultColor);
                        context.Output.Write($"{padding}{Indent}{second}");
                        ConsoleExtensions.SetColor(theme.DefaultColor);
                    }

                    context.Output.WriteLine();
                }
            }

            static IEnumerable<(string, string)> ZipWithEmpty(IEnumerable<string> first, IEnumerable<string> second)
            {
                using var enum1 = first.GetEnumerator();
                using var enum2 = second.GetEnumerator();
                bool hasFirst = false, hasSecond = false;
                while ((hasFirst = enum1.MoveNext()) | (hasSecond = enum2.MoveNext()))
                {
                    yield return (hasFirst ? enum1.Current : "", hasSecond ? enum2.Current : "");
                }
            }
        }

        private string GetUsage(Command command)
        {
            return string.Join(" ", GetUsageParts().Where(x => !string.IsNullOrWhiteSpace(x)));

            IEnumerable<string> GetUsageParts()
            {
                bool displayOptionTitle = false;

                IEnumerable<Command> parentCommands =
                    command
                        .RecurseWhileNotNull(c => c.Parents.OfType<Command>().FirstOrDefault())
                        .Reverse();

                foreach (var parentCommand in parentCommands)
                {
                    if (!displayOptionTitle)
                    {
                        displayOptionTitle = parentCommand.HasOptions() && parentCommand.Options.Any(x => x.Recursive && !x.Hidden);
                    }

                    /*MODIFY*/
                    //If non-root command is run directly or root command has custom name, show the executable name
                    if (parentCommand.Parents.FirstOrDefault() == null)
                        yield return ExecutableInfo.ExecutableName;
                    else
                    /*MODIFY*/
                        yield return parentCommand.Name;

                    if (parentCommand.HasArguments())
                    {
                        yield return FormatArgumentUsage(parentCommand.Arguments);
                    }
                }

                var hasCommandWithHelp = command.HasSubcommands() && command.Subcommands.Any(x => !x.Hidden);

                if (hasCommandWithHelp)
                {
                    yield return LocalizationResources.HelpUsageCommand();
                }

                displayOptionTitle = displayOptionTitle || (command.HasOptions() && command.Options.Any(x => !x.Hidden));

                if (displayOptionTitle)
                {
                    yield return LocalizationResources.HelpUsageOptions();
                }

                if (!command.TreatUnmatchedTokensAsErrors)
                {
                    yield return LocalizationResources.HelpUsageAdditionalArguments();
                }
            }
        }

        private string FormatArgumentUsage(IList<Argument> arguments)
        {
            var sb = new StringBuilder(arguments.Count * 100);

            var end = default(List<char>);

            for (var i = 0; i < arguments.Count; i++)
            {
                var argument = arguments[i];
                if (argument.Hidden)
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
                    (end ??= new()).Add(']');
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

                if (end is { })
                {
                    while (end.Count > 0)
                    {
                        sb.Append(end[end.Count - 1]);
                        end.RemoveAt(end.Count - 1);
                    }
                }
            }

            return sb.ToString();

            bool IsOptional(Argument argument) =>
                argument.Arity.MinimumNumberOfValues == 0;
        }

        private static IEnumerable<string> WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield break;
            }

            //First handle existing new lines
            var parts = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string part in parts)
            {
                if (part.Length <= maxWidth)
                {
                    yield return part;
                }
                else
                {
                    //Long item, wrap it based on the width
                    for (int i = 0; i < part.Length;)
                    {
                        if (part.Length - i < maxWidth)
                        {
                            yield return part.Substring(i);
                            break;
                        }
                        else
                        {
                            int length = -1;
                            for (int j = 0; j + i < part.Length && j < maxWidth; j++)
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

        private IEnumerable<TwoColumnHelpRow> GetCommandArgumentRows(Command command, HelpContext context) =>
            command
                .RecurseWhileNotNull(c => c.Parents.OfType<Command>().FirstOrDefault())
                .Reverse()
                .SelectMany(cmd => cmd.Arguments.Where(a => !a.Hidden))
                .Select(a => GetTwoColumnRow(a, context))
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

            Customization customization = null;

            if (_customizationsBySymbol is { })
            {
                _customizationsBySymbol.TryGetValue(symbol, out customization);
            }

            if (symbol is Option or Command)
            {
                return GetOptionOrCommandRow();
            }
            else if (symbol is Argument argument)
            {
                return GetCommandArgumentRow(argument);
            }
            else
            {
                throw new NotSupportedException($"Symbol type {symbol.GetType()} is not supported.");
            }

            TwoColumnHelpRow GetOptionOrCommandRow()
            {
                var firstColumnText = customization?.GetFirstColumn?.Invoke(context)
                    ?? (symbol is Option option
                            ? Default.GetOptionUsageLabel(option)
                            : Default.GetCommandUsageLabel((Command)symbol));

                var customizedSymbolDescription = customization?.GetSecondColumn?.Invoke(context);

                var symbolDescription = customizedSymbolDescription ?? symbol.Description ?? string.Empty;

                //in case symbol description is customized, do not output default value
                //default value output is not customizable for identifier symbols
                var defaultValueDescription = customizedSymbolDescription == null
                    ? GetSymbolDefaultValue(symbol)
                    : string.Empty;

                /*MODIFY*/
                if (symbol is Option o && o.Required)
                {
                    if (symbolDescription.Length > 0)
                        symbolDescription += " ";

                    symbolDescription += GetRequiredLabel();
                }
                if (defaultValueDescription == "[]")
                    defaultValueDescription = string.Empty;
                /*MODIFY*/

                var secondColumnText = $"{symbolDescription} {defaultValueDescription}".Trim();

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }

            TwoColumnHelpRow GetCommandArgumentRow(Argument argument)
            {
                var firstColumnText =
                    customization?.GetFirstColumn?.Invoke(context) ?? Default.GetArgumentUsageLabel(argument);

                var argumentDescription =
                    customization?.GetSecondColumn?.Invoke(context) ?? Default.GetArgumentDescription(argument);

                var defaultValueDescription =
                    argument.HasDefaultValue
                        ? $"[{GetArgumentDefaultValue(context.Command, argument, true, context)}]"
                        : "";

                /*MODIFY*/
                if (!argument.HasDefaultValue)
                {
                    if (argumentDescription.Length > 0)
                        argumentDescription += " ";

                    argumentDescription += GetRequiredLabel();
                }
                if (defaultValueDescription == "[]")
                    defaultValueDescription = string.Empty;
                /*MODIFY*/

                var secondColumnText = $"{argumentDescription} {defaultValueDescription}".Trim();

                return new TwoColumnHelpRow(firstColumnText, secondColumnText);
            }

            string GetSymbolDefaultValue(Symbol symbol)
            {
                var arguments = symbol.GetParameters();
                var defaultArguments = arguments.Where(x => !x.Hidden && (x is Argument { HasDefaultValue: true } || x is Option { HasDefaultValue: true })).ToArray();

                if (defaultArguments.Length == 0) return "";

                var isSingleArgument = defaultArguments.Length == 1;
                var argumentDefaultValues = defaultArguments
                    .Select(argument => GetArgumentDefaultValue(symbol, argument, isSingleArgument, context));
                return $"[{string.Join(", ", argumentDefaultValues)}]";
            }
        }
        // ReSharper disable once InconsistentNaming
        private Dictionary<Symbol, Customization> _customizationsBySymbol = null;

        private string GetArgumentDefaultValue(
            Symbol parent,
            Symbol parameter,
            bool displayArgumentName,
            HelpContext context)
        {
            string label = displayArgumentName
                ? LocalizationResources.HelpArgumentDefaultValueLabel()
                : parameter.Name;

            string displayedDefaultValue = null;

            if (_customizationsBySymbol is not null)
            {
                if (_customizationsBySymbol.TryGetValue(parent, out var customization) &&
                    customization.GetDefaultValue?.Invoke(context) is { } parentDefaultValue)
                {
                    displayedDefaultValue = parentDefaultValue;
                }
                else if (_customizationsBySymbol.TryGetValue(parameter, out customization) &&
                         customization.GetDefaultValue?.Invoke(context) is { } ownDefaultValue)
                {
                    displayedDefaultValue = ownDefaultValue;
                }
            }

            displayedDefaultValue ??= Default.GetArgumentDefaultValue(parameter);

            if (string.IsNullOrWhiteSpace(displayedDefaultValue))
            {
                return "";
            }

            return $"{label}: {displayedDefaultValue}";
        }

        /// <summary>
        /// Provides default formatting for help output.
        /// </summary>
        public new static class Default
        {
            /// <summary>
            /// Gets an argument's default value to be displayed in help.
            /// </summary>
            /// <param name="parameter">The argument to get the default value for.</param>
            /// <returns>Argument default value.</returns>
            public static string GetArgumentDefaultValue(Symbol parameter) => HelpBuilder.Default.GetArgumentDefaultValue(parameter);

            /// <summary>
            /// Gets the description for an argument (typically used in the second column text in the arguments section).
            /// </summary>
            /// <param name="argument">The argument to get the default value for.</param>
            /// <returns>Argument description.</returns>
            public static string GetArgumentDescription(Argument argument) => HelpBuilder.Default.GetArgumentDescription(argument);

            /// <summary>
            /// Gets the usage title for an argument (for example: <c>&lt;value&gt;</c>, typically used in the first column text in the arguments usage section, or within the synopsis.
            /// </summary>
            /// <param name="parameter">The argument to get the default value for.</param>
            /// <returns>Argument usage label.</returns>
            public static string GetArgumentUsageLabel(Symbol parameter) => HelpBuilder.Default.GetArgumentUsageLabel(parameter);

            /// <summary>
            /// Gets the usage label for the specified symbol (typically used as the first column text in help output).
            /// </summary>
            /// <param name="symbol">The symbol to get a help item for.</param>
            /// <returns>Text to display.</returns>
            public static string GetCommandUsageLabel(Command symbol)
                => GetIdentifierSymbolUsageLabel(symbol, symbol.Aliases);

            /// <inheritdoc cref="GetCommandUsageLabel(Command)"/>
            public static string GetOptionUsageLabel(Option symbol)
                => GetIdentifierSymbolUsageLabel(symbol, symbol.Aliases);

            private static string GetIdentifierSymbolUsageLabel(Symbol symbol, ICollection<string> aliasSet)
            {
                var aliases = aliasSet.Count == 0 /*MODIFY*/
                    ? new[] { symbol.Name }
                    : new[] { symbol.Name }.Concat(aliasSet)
                                    .Select(r => r.SplitPrefix())
                                    .OrderBy(r => r.Prefix, StringComparer.OrdinalIgnoreCase)
                                    .ThenBy(r => r.Alias.Length) /*MODIFY*/
                                    //.ThenBy(r => r.Alias, StringComparer.OrdinalIgnoreCase) /*MODIFY*/
                                    .GroupBy(t => t.Alias)
                                    .Select(t => t.First())
                                    .Select(t => $"{t.Prefix}{t.Alias}");

                var firstColumnText = string.Join(", ", aliases);

                /*MODIFY*/
                if (symbol is Command)
                    return firstColumnText;
                /*MODIFY*/

                foreach (var argument in symbol.GetParameters())
                {
                    if (!argument.Hidden)
                    {
                        var argumentFirstColumnText = Default.GetArgumentUsageLabel(argument);

                        if (!string.IsNullOrWhiteSpace(argumentFirstColumnText))
                        {
                            firstColumnText += $" {argumentFirstColumnText}";
                        }
                    }
                }

                /*MODIFY*/
                /*
                if (symbol is CliOption { Required: true })
                {
                    firstColumnText += $" {LocalizationResources.HelpOptionsRequiredLabel()}";
                }
                */
                /*MODIFY*/

                return firstColumnText;
            }
        }

        private static string GetRequiredLabel()
        {
            var label =  LocalizationResources.HelpOptionsRequiredLabel()
                .TrimStart('(').TrimEnd(')').ToLowerInvariant();

            return $"[{label}]";
        }

        private class Customization
        {
            public Customization(
                Func<HelpContext, string> getFirstColumn,
                Func<HelpContext, string> getSecondColumn,
                Func<HelpContext, string> getDefaultValue)
            {
                GetFirstColumn = getFirstColumn;
                GetSecondColumn = getSecondColumn;
                GetDefaultValue = getDefaultValue;
            }

            public Func<HelpContext, string> GetFirstColumn { get; }
            public Func<HelpContext, string> GetSecondColumn { get; }
            public Func<HelpContext, string> GetDefaultValue { get; }
        }
    }
}
