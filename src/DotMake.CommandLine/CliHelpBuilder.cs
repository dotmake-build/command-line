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
        private static readonly MethodInfo IsGlobalGetter = typeof(Option)
            .GetProperty("IsGlobal", BindingFlags.NonPublic | BindingFlags.Instance)?.GetMethod;
        private static readonly MethodInfo WasSectionSkippedSetter = typeof(HelpContext)
            .GetProperty("WasSectionSkipped", BindingFlags.NonPublic | BindingFlags.Instance)?.SetMethod;
        private readonly IConsole console;


        /// <summary>
        /// Initializes a new instance of the <see cref="CliHelpBuilder" /> class.
        /// </summary>
        /// <param name="localizationResources">Resources used to localize the help output.</param>
        /// <param name="maxWidth">The maximum width in characters after which help output is wrapped.</param>
        /// <param name="console">The console to which output should be written during the current invocation.</param>
        public CliHelpBuilder(LocalizationResources localizationResources, int maxWidth = int.MaxValue, IConsole console = null)
            : base(localizationResources, maxWidth)
        {
            CustomizeLayout(GetLayout);
            this.console = console;
        }

        /// <summary>
        /// Gets the default sections to be written for command line help.
        /// </summary>
        public virtual IEnumerable<HelpSectionDelegate> GetLayout(HelpContext helpContext)
        {
            yield return WriteSynopsisSection;
            yield return WriteCommandUsageSection;
            yield return WriteCommandArgumentsSection;
            yield return WriteOptionsSection;
            yield return WriteSubcommandsSection;
            yield return WriteAdditionalArgumentsSection;
        }

        /*
        /// <summary>
        /// Writes help output for the specified command.
        /// </summary>
        public override void Write(HelpContext helpContext)
        {
            if (helpContext is null)
            {
                throw new ArgumentNullException(nameof(helpContext));
            }

            if (helpContext.Command.IsHidden)
            {
                return;
            }

            
            //if (OnCustomize is { })
            //{
            //    OnCustomize(helpContext);
            //}
            

            foreach (var writeSection in GetLayout(helpContext))
            {
                writeSection(helpContext);

                if (!helpContext.WasSectionSkipped)
                {
                    helpContext.Output.WriteLine();
                }
            }

            helpContext.Output.WriteLine();
        }
        */

        /// <summary>
        /// Writes a help section describing a command's synopsis.
        /// </summary>
        public virtual void WriteSynopsisSection(HelpContext helpContext)
        {
            helpContext.Output.Write(ExecutableInfo.Product);
            if (!string.IsNullOrWhiteSpace(ExecutableInfo.Version))
                helpContext.Output.Write(" v{0}", ExecutableInfo.Version);
            helpContext.Output.WriteLine();
            if (!string.IsNullOrWhiteSpace(ExecutableInfo.Copyright))
                helpContext.Output.WriteLine(ExecutableInfo.Copyright);
            helpContext.Output.WriteLine();

            var isRoot = (helpContext.Command.Parents.FirstOrDefault() == null);
            var commandName = isRoot ? string.Empty : helpContext.Command.Name;
            var description = isRoot ? helpContext.Command.Description : ": " + helpContext.Command.Description;

            if (commandName.Length > 0)
            {
                console.SetForegroundColor(ConsoleColor.White);
                helpContext.Output.Write(commandName);
                console.ResetForegroundColor();
            }
            foreach (var part in WrapText(description, MaxWidth - commandName.Length))
                helpContext.Output.WriteLine(part);

            /*
            WriteHeading(
                LocalizationResources.HelpDescriptionTitle(),
                helpContext.Command.Description,
                helpContext.Output
            );*/
        }

        /// <summary>
        /// Writes a help section describing a command's usage.
        /// </summary>
        public virtual void WriteCommandUsageSection(HelpContext helpContext)
        {
            var usage = string.Join(" ", GetUsageParts(helpContext.Command).Where(x => !string.IsNullOrWhiteSpace(x)));

            WriteHeading(
                LocalizationResources.HelpUsageTitle(),
                usage,
                helpContext.Output
            );
        }

        ///  <summary>
        /// Writes a help section describing a command's arguments.
        ///  </summary>
        public virtual void WriteCommandArgumentsSection(HelpContext helpContext)
        {
            var commandArguments = GetCommandArgumentRows(helpContext.Command, helpContext).ToArray();

            WasSectionSkippedSetter.Invoke(helpContext, new object[] { false });
            if (commandArguments.Length <= 0)
            {
                WasSectionSkippedSetter.Invoke(helpContext, new object[] { true });
                return;
            }

            WriteHeading(
                LocalizationResources.HelpArgumentsTitle(),
                null,
                helpContext.Output
            );

            WriteColumns(commandArguments, helpContext);
        }

        ///  <summary>
        /// Writes a help section describing a command's options.
        ///  </summary>
        public virtual void WriteOptionsSection(HelpContext helpContext)
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

            WasSectionSkippedSetter.Invoke(helpContext, new object[] { false });
            if (options.Count <= 0)
            {
                WasSectionSkippedSetter.Invoke(helpContext, new object[] { true });
                return;
            }

            WriteHeading(
                LocalizationResources.HelpOptionsTitle(),
                null,
                helpContext.Output
            );

            WriteColumns(options, helpContext);
        }

        ///  <summary>
        /// Writes a help section describing a command's subcommands.
        ///  </summary>
        public virtual void WriteSubcommandsSection(HelpContext helpContext)
        {
            var subcommands = helpContext.Command.Subcommands
                .Where(x => !x.IsHidden)
                .Select(x => GetTwoColumnRow(x, helpContext)).ToArray();

            WasSectionSkippedSetter.Invoke(helpContext, new object[] { false });
            if (subcommands.Length <= 0)
            {
                WasSectionSkippedSetter.Invoke(helpContext, new object[] { true });
                return;
            }

            WriteHeading(LocalizationResources.HelpCommandsTitle(), null, helpContext.Output);

            WriteColumns(subcommands, helpContext);
        }
        
        ///  <summary>
        /// Writes a help section describing a command's additional arguments, typically shown only when <see cref="Command.TreatUnmatchedTokensAsErrors"/> is set to <see langword="true"/>.
        ///  </summary>
        public virtual void WriteAdditionalArgumentsSection(HelpContext helpContext)
        {
            WasSectionSkippedSetter.Invoke(helpContext, new object[] { false });
            if (helpContext.Command.TreatUnmatchedTokensAsErrors)
            {
                WasSectionSkippedSetter.Invoke(helpContext, new object[] { true });
                return;
            }

            WriteHeading(
                LocalizationResources.HelpAdditionalArgumentsTitle(),
                LocalizationResources.HelpAdditionalArgumentsDescription(),
                helpContext.Output
            );
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

        private IEnumerable<TwoColumnHelpRow> GetCommandArgumentRows(Command command, HelpContext helpContext) =>
            command
                .RecurseWhileNotNull(c => c.Parents.OfType<Command>().FirstOrDefault())
                .Reverse()
                .SelectMany(cmd => cmd.Arguments.Where(a => !a.IsHidden))
                .Select(a => GetTwoColumnRow(a, helpContext))
                .Distinct();
        
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
    }
}
