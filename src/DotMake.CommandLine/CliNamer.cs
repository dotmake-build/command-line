using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides methods for generating CLI names and aliases while tracking already used ones.
    /// </summary>
    public class CliNamer
    {
        private readonly CliNameAutoGenerate nameAutoGenerate;
        private readonly CliNameCasingConvention nameCasingConvention;
        private readonly CliNamePrefixConvention namePrefixConvention;
        private readonly CliNamePrefixConvention shortFormPrefixConvention;
        private readonly CliNameAutoGenerate shortFormAutoGenerate;
        private readonly CliNamer parentNamer;
        private readonly Dictionary<string, Tuple<TokenType, string>> usedTokens = new(StringComparer.Ordinal);

        private static readonly string[] CommandSuffixes = { "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli" };
        private static readonly string[] DirectiveSuffixes = CommandSuffixes
            .Select(s => s + "Directive")
            .Append("Directive")
            .ToArray();
        private static readonly string[] OptionSuffixes = CommandSuffixes
            .Select(s => s + "Option")
            .Append("Option")
            .ToArray();
        private static readonly string[] ArgumentSuffixes = CommandSuffixes
            .Select(s => s + "Argument")
            .Append("Argument")
            .ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="CliNamer" /> class.
        /// </summary>
        /// <param name="nameAutoGenerate">A value which indicates whether names are automatically generated for commands, directives, options and arguments.</param>
        /// <param name="nameCasingConvention">The character casing convention used for automatically generated command, option and argument names.</param>
        /// <param name="namePrefixConvention">The prefix convention used for automatically generated option names.</param>
        /// <param name="shortFormPrefixConvention">The prefix convention used for automatically generated short form option aliases.</param>
        /// <param name="shortFormAutoGenerate">A value which indicates whether short form aliases are automatically generated for commands and options.</param>
        /// <param name="parentNamer">The parent namer used to check names and aliases of sub-commands.</param>
        public CliNamer(
            CliNameAutoGenerate? nameAutoGenerate = null,
            CliNameCasingConvention? nameCasingConvention = null,
            CliNamePrefixConvention? namePrefixConvention = null,
            CliNameAutoGenerate? shortFormAutoGenerate = null,
            CliNamePrefixConvention? shortFormPrefixConvention = null,
            CliNamer parentNamer = null)
        {
            this.nameAutoGenerate = nameAutoGenerate ?? CliCommandAttribute.Default.NameAutoGenerate;
            this.nameCasingConvention = nameCasingConvention ?? CliCommandAttribute.Default.NameCasingConvention;
            this.namePrefixConvention = namePrefixConvention ?? CliCommandAttribute.Default.NamePrefixConvention;
            this.shortFormAutoGenerate = shortFormAutoGenerate ?? CliCommandAttribute.Default.ShortFormAutoGenerate;
            this.shortFormPrefixConvention = shortFormPrefixConvention ?? CliCommandAttribute.Default.ShortFormPrefixConvention;
            this.parentNamer = parentNamer;
        }

        /// <summary>
        /// Gets the command name for a property by using current <see cref="nameCasingConvention"/>.
        /// </summary>
        public string GetCommandName(string symbolName, string specificName = null)
        {
            //Commands are added with name in ValidTokens so commands can conflict with options without prefix.

            //Note that currently this method is only called for the command itself in the Build method because children commands
            //are not yet known at the time (as CliNamer scoped to Build method, can not add children)

            if (!string.IsNullOrWhiteSpace(specificName))
            {
                specificName = specificName.Trim();
                AddTokenOrThrow(specificName, TokenType.CommandName, symbolName);
                return specificName;
            }

            var baseName = symbolName.Trim().StripSuffixes(CommandSuffixes);

            var name = nameAutoGenerate.HasFlag(CliNameAutoGenerate.Commands)
                ? FindAutoName(baseName, false)
                : baseName;
            AddTokenOrThrow(name, TokenType.CommandName, symbolName);
            return name;
        }

        /// <summary>
        /// Gets the directive name for a property by using current <see cref="nameCasingConvention"/>.
        /// </summary>
        public string GetDirectiveName(string symbolName, string specificName = null)
        {
            //Directives are added with [] around name in ValidTokens so they can conflict with other symbols with [].

            if (!string.IsNullOrWhiteSpace(specificName))
            {
                specificName = specificName.Trim();
                AddTokenOrThrow(specificName, TokenType.DirectiveName, symbolName);
                return specificName;
            }

            var baseName = symbolName.Trim().StripSuffixes(DirectiveSuffixes);

            var name = nameAutoGenerate.HasFlag(CliNameAutoGenerate.Directives)
                ? baseName.ToCase(nameCasingConvention)
                : baseName;
            AddTokenOrThrow(name, TokenType.DirectiveName, symbolName);
            return name;
        }

        /// <summary>
        /// Gets the option name for a property by using current <see cref="nameCasingConvention"/> and <see cref="namePrefixConvention"/>.
        /// </summary>
        public string GetOptionName(string symbolName, string specificName = null)
        {
            //Options are added with name in ValidTokens so options without prefix can conflict with commands.

            if (!string.IsNullOrWhiteSpace(specificName))
            {
                specificName = specificName.Trim();
                specificName = specificName.AddPrefix(namePrefixConvention); //will ignore if already has a prefix
                AddTokenOrThrow(specificName, TokenType.OptionName, symbolName);
                return specificName;
            }

            var baseName = symbolName.Trim().StripSuffixes(OptionSuffixes);

            var name = nameAutoGenerate.HasFlag(CliNameAutoGenerate.Options)
                ? FindAutoName(baseName, true)
                : baseName;
            AddTokenOrThrow(name, TokenType.OptionName, symbolName);
            return name;
        }

        /// <summary>
        /// Gets the argument name for a property by using current <see cref="nameCasingConvention"/>.
        /// </summary>
        public string GetArgumentName(string symbolName, string specificName = null)
        {
            //Arguments are not added in ValidTokens so they won't conflict with other symbols.

            if (!string.IsNullOrWhiteSpace(specificName))
            {
                specificName = specificName.Trim();
                return specificName;
            }

            var baseName = symbolName.Trim().StripSuffixes(ArgumentSuffixes);

            return nameAutoGenerate.HasFlag(CliNameAutoGenerate.Arguments)
                ? baseName.ToCase(nameCasingConvention)
                : baseName;
        }

        /// <summary>
        /// Adds an alias to a command. Tracks used aliases and only adds if not already used.
        /// </summary>
        public void AddAlias(Command command, string symbolName, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return;

            AddTokenOrThrow(alias, TokenType.CommandAlias, symbolName);
            command.Aliases.Add(alias);
        }

        /// <summary>
        /// Adds an alias to an option. Tracks used aliases and only adds if not already used.
        /// </summary>
        public void AddAlias(Option option, string symbolName, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return;

            alias = alias.AddPrefix(namePrefixConvention); //will ignore if already has a prefix

            AddTokenOrThrow(alias, TokenType.OptionAlias, symbolName);
            option.Aliases.Add(alias);
        }

        /// <summary>
        /// Adds a short form alias for a command name for a property by using current <see cref="nameCasingConvention"/>.
        /// <para>
        /// Short form alias is added only when current <see cref="shortFormAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Commands"/>
        /// and it is shorter than command name.
        /// </para>
        /// </summary>
        public void AddShortFormAlias(Command command, string symbolName, string specificAlias = null)
        {
            if (!string.IsNullOrWhiteSpace(specificAlias))
            {
                specificAlias = specificAlias.Trim();
                AddAlias(command, symbolName, specificAlias);
            }
            else
            {
                var baseName = symbolName.Trim().StripSuffixes(CommandSuffixes);

                var shortForm = shortFormAutoGenerate.HasFlag(CliNameAutoGenerate.Commands)
                    ? FindAutoShortForm(baseName, false)
                    : baseName;

                if (shortForm.Length != 0 && shortForm.Length < command.Name.Length)
                    AddAlias(command, symbolName, shortForm);
            }
        }

        /// <summary>
        /// Adds a short form alias for an option name for a property by using current <see cref="nameCasingConvention"/> and <see cref="shortFormPrefixConvention"/>.
        /// <para>
        /// Short form alias is added only when current <see cref="shortFormAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Options"/>
        /// and it is shorter than option name.
        /// </para>
        /// </summary>
        public void AddShortFormAlias(Option option, string symbolName, string specificAlias = null)
        {
            if (!string.IsNullOrWhiteSpace(specificAlias))
            {
                specificAlias = specificAlias.Trim();
                specificAlias = specificAlias.AddPrefix(shortFormPrefixConvention); //will ignore if already has a prefix
                AddAlias(option, symbolName, specificAlias);
            }
            else
            {
                var baseName = symbolName.Trim().StripSuffixes(OptionSuffixes);

                var shortForm = shortFormAutoGenerate.HasFlag(CliNameAutoGenerate.Options)
                    ? FindAutoShortForm(baseName, true)
                    : baseName;

                if (shortForm.Length != 0 && shortForm.Length < option.Name.Length)
                    AddAlias(option, symbolName, shortForm);
            }
        }

        private string FindAutoName(string baseName, bool withPrefix)
        {
            for (var i = 0; i < 5; i++)
            {
                var name = (i == 0)
                    ? baseName
                    : baseName + "-" + (i + 1);
                name = name.ToCase(nameCasingConvention);
                if (withPrefix)
                    name = name.AddPrefix(namePrefixConvention);

                if (usedTokens.ContainsKey(name))
                    continue;

                return name;
            }

            return baseName.ToCase(nameCasingConvention);
        }

        private string FindAutoShortForm(string baseName, bool withPrefix)
        {
            var words = baseName.SplitWords();

            var shortForm = "";
            foreach (var word in words)
            {
                shortForm += int.TryParse(word, out var number) //treat numbers as special, e.g. 256 should not be reduced to 2
                    ? number.ToString()
                    : word?[0].ToString().ToCase(nameCasingConvention);
            }

            if (withPrefix)
                shortForm = shortForm.AddPrefix(shortFormPrefixConvention);

            return shortForm;
        }

        private void AddTokenOrThrow(string token, TokenType tokenType, string symbolName)
        {
            if (tokenType == TokenType.DirectiveName)
                token = $"[{token}]";

            if (usedTokens.TryGetValue(token, out var tuple))
            {
                var existingTokenType = tuple.Item1;
                var existingSymbolName = tuple.Item2;
                throw new Exception(
                    $"{tokenType} '{token}' for '{symbolName}' conflicts with {existingTokenType} for '{existingSymbolName}'!"
                );
            }

            if (parentNamer != null
                && (tokenType == TokenType.CommandName || tokenType == TokenType.CommandAlias)
                && parentNamer.usedTokens.TryGetValue(token, out var tuple2))
            {
                var existingTokenType = tuple2.Item1;
                var existingSymbolName = tuple2.Item2;
                throw new Exception(
                    $"{tokenType} '{token}' for '{symbolName}' conflicts with parent {existingTokenType} for '{existingSymbolName}'!"
                );
            }

            usedTokens.Add(token, Tuple.Create(tokenType, symbolName));
        }

        private enum TokenType
        {
            DirectiveName,
            CommandName,
            CommandAlias,
            OptionName,
            OptionAlias
        }
    }
}
