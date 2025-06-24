using System;
using System.Collections.Generic;
using System.CommandLine;

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
        private readonly HashSet<string> usedNames = new(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="CliNamer" /> class.
        /// </summary>
        /// <param name="nameAutoGenerate">A value which indicates whether names are automatically generated for commands, directives, options and arguments.</param>
        /// <param name="nameCasingConvention">The character casing convention used for automatically generated command, option and argument names.</param>
        /// <param name="namePrefixConvention">The prefix convention used for automatically generated option names.</param>
        /// <param name="shortFormPrefixConvention">The prefix convention used for automatically generated short form option aliases.</param>
        /// <param name="shortFormAutoGenerate">A value which indicates whether short form aliases are automatically generated for commands and options.</param>
        public CliNamer(
            CliNameAutoGenerate? nameAutoGenerate = null,
            CliNameCasingConvention? nameCasingConvention = null,
            CliNamePrefixConvention? namePrefixConvention = null,
            CliNameAutoGenerate? shortFormAutoGenerate = null,
            CliNamePrefixConvention? shortFormPrefixConvention = null)
        {
            this.nameAutoGenerate = nameAutoGenerate ?? CliCommandAttribute.Default.NameAutoGenerate;
            this.nameCasingConvention = nameCasingConvention ?? CliCommandAttribute.Default.NameCasingConvention;
            this.namePrefixConvention = namePrefixConvention ?? CliCommandAttribute.Default.NamePrefixConvention;
            this.shortFormAutoGenerate = shortFormAutoGenerate ?? CliCommandAttribute.Default.ShortFormAutoGenerate;
            this.shortFormPrefixConvention = shortFormPrefixConvention ?? CliCommandAttribute.Default.ShortFormPrefixConvention;
        }

        /// <summary>
        /// Gets the command name for a property by using current <see cref="nameCasingConvention"/>.
        /// </summary>
        public string GetCommandName(string baseName, bool isSpecificName)
        {
            //Commands are added with name in ValidTokens so commands can conflict with options without prefix.

            //Note that currently this method is only called for the command itself in the Build method because children commands
            //are not yet known at the time (as CliNamer scoped to Build method, can not add children)

            if (isSpecificName || !nameAutoGenerate.HasFlag(CliNameAutoGenerate.Commands))
            {
                usedNames.Add(baseName);
                return baseName;
            }

            var name = FindAutoName(baseName, false);
            usedNames.Add(name);
            return name;
        }

        /// <summary>
        /// Gets the directive name for a property by using current <see cref="nameCasingConvention"/>.
        /// </summary>
        public string GetDirectiveName(string baseName, bool isSpecificName)
        {
            //Directives are added with [] around name in ValidTokens so they won't conflict with other symbols.

            if (isSpecificName || !nameAutoGenerate.HasFlag(CliNameAutoGenerate.Directives))
                return baseName;

            return baseName.ToCase(nameCasingConvention);
        }

        /// <summary>
        /// Gets the option name for a property by using current <see cref="nameCasingConvention"/> and <see cref="namePrefixConvention"/>.
        /// </summary>
        public string GetOptionName(string baseName, bool isSpecificName)
        {
            //Options are added with name in ValidTokens so options without prefix can conflict with commands.

            if (isSpecificName || !nameAutoGenerate.HasFlag(CliNameAutoGenerate.Options))
            {
                baseName = baseName.AddPrefix(namePrefixConvention); //will ignore if already has a prefix
                usedNames.Add(baseName);
                return baseName;
            }

            var name = FindAutoName(baseName, true);
            usedNames.Add(name);
            return name;
        }

        /// <summary>
        /// Gets the argument name for a property by using current <see cref="nameCasingConvention"/>.
        /// </summary>
        public string GetArgumentName(string baseName, bool isSpecificName)
        {
            //Arguments are not added in ValidTokens so they won't conflict with other symbols.

            if (isSpecificName || !nameAutoGenerate.HasFlag(CliNameAutoGenerate.Arguments))
                return baseName;

            return baseName.ToCase(nameCasingConvention);
        }

        /// <summary>
        /// Adds an alias to a command. Tracks used aliases and only adds if not already used.
        /// </summary>
        public void AddAlias(Command command, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return;

            if (!usedNames.Contains(alias))
            {
                command.Aliases.Add(alias);
                usedNames.Add(alias);
            }
        }

        /// <summary>
        /// Adds an alias to an option. Tracks used aliases and only adds if not already used.
        /// </summary>
        public void AddAlias(Option option, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return;

            alias = alias.AddPrefix(namePrefixConvention); //will ignore if already has a prefix

            if (!usedNames.Contains(alias))
            {
                option.Aliases.Add(alias);
                usedNames.Add(alias);
            }
        }

        /// <summary>
        /// Adds a short form alias for a command name for a property by using current <see cref="nameCasingConvention"/>.
        /// <para>
        /// Short form alias is added only when current <see cref="shortFormAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Commands"/>
        /// and it is shorter than command name.
        /// </para>
        /// </summary>
        public void AddShortFormAlias(Command command, string baseName, bool isSpecificName)
        {
            if (isSpecificName || !shortFormAutoGenerate.HasFlag(CliNameAutoGenerate.Commands))
            {
                AddAlias(command, baseName);
            }
            else
            {
                var shortForm = FindAutoShortForm(baseName, false);

                if (shortForm.Length != 0 && shortForm.Length < command.Name.Length)
                    AddAlias(command, shortForm);
            }
        }

        /// <summary>
        /// Adds a short form alias for an option name for a property by using current <see cref="nameCasingConvention"/> and <see cref="shortFormPrefixConvention"/>.
        /// <para>
        /// Short form alias is added only when current <see cref="shortFormAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Options"/>
        /// and it is shorter than option name.
        /// </para>
        /// </summary>
        public void AddShortFormAlias(Option option, string baseName, bool isSpecificName)
        {
            if (isSpecificName || !shortFormAutoGenerate.HasFlag(CliNameAutoGenerate.Options))
            {
                baseName = baseName.AddPrefix(shortFormPrefixConvention); //will ignore if already has a prefix
                AddAlias(option, baseName);
            }
            else
            {
                var shortForm = FindAutoShortForm(baseName, true);

                if (shortForm.Length != 0 && shortForm.Length < option.Name.Length)
                    AddAlias(option, shortForm);
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

                if (usedNames.Contains(name))
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
    }
}
