using System;
using System.Collections.Generic;
using System.Linq;
using DotMake.CommandLine.Util;

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
        private readonly Dictionary<string, HashSet<NameInfo>> symbolMappings = new(StringComparer.Ordinal);

        private readonly Dictionary<int, Dictionary<string, NameInfo>> usedTokens = new();

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
            this.nameAutoGenerate = nameAutoGenerate ?? parentNamer?.nameAutoGenerate ?? CliCommandAttribute.Default.NameAutoGenerate;
            this.nameCasingConvention = nameCasingConvention ?? parentNamer?.nameCasingConvention ?? CliCommandAttribute.Default.NameCasingConvention;
            this.namePrefixConvention = namePrefixConvention ?? parentNamer ?.namePrefixConvention ?? CliCommandAttribute.Default.NamePrefixConvention;
            this.shortFormAutoGenerate = shortFormAutoGenerate ?? parentNamer ?.shortFormAutoGenerate ?? CliCommandAttribute.Default.ShortFormAutoGenerate;
            this.shortFormPrefixConvention = shortFormPrefixConvention ?? parentNamer?.shortFormPrefixConvention ?? CliCommandAttribute.Default.ShortFormPrefixConvention;
            this.parentNamer = parentNamer;
        }


        /// <summary>
        /// Adds a command symbol which is used for auto generated or specific names/aliases.
        /// </summary>
        public void AddCommandSymbol(string symbolName, string specificName = null, string specificShortAlias = null, string[] specificAliases = null)
        {
            //Do not add names or aliases for root commands as it's never used and it can unnecessarily conflict with children
            //Auto short aliases will also not be generated as we don't add a CommandName
            if (parentNamer == null)
                return;

            AddSymbolMapping(symbolName, NameType.CommandName, specificName);

            if (!string.IsNullOrWhiteSpace(specificShortAlias))
                AddSymbolMapping(symbolName, NameType.CommandShortAlias, specificShortAlias);

            if (specificAliases != null)
            {
                foreach (var specificAlias in specificAliases)
                {
                    if (!string.IsNullOrWhiteSpace(specificAlias))
                        AddSymbolMapping(symbolName, NameType.CommandAlias, specificAlias);
                }
            }
        }

        /// <summary>
        /// Adds an option symbol which is used for auto generated or specific names/aliases.
        /// </summary>
        public void AddOptionSymbol(string symbolName, string specificName = null, string specificShortAlias = null, string[] specificAliases = null)
        {
            AddSymbolMapping(symbolName, NameType.OptionName, specificName);

            if (!string.IsNullOrWhiteSpace(specificShortAlias))
                AddSymbolMapping(symbolName, NameType.OptionShortAlias, specificShortAlias);

            if (specificAliases != null)
            {
                foreach (var specificAlias in specificAliases)
                {
                    if (!string.IsNullOrWhiteSpace(specificAlias))
                        AddSymbolMapping(symbolName, NameType.OptionAlias, specificAlias);
                }
            }
        }

        /// <summary>
        /// Adds an argument symbol which is used for auto generated or specific names.
        /// </summary>
        public void AddArgumentSymbol(string symbolName, string specificName = null)
        {
            AddSymbolMapping(symbolName, NameType.ArgumentName, specificName);
        }

        /// <summary>
        /// Adds a directive symbol which is used for auto generated or specific names.
        /// </summary>
        public void AddDirectiveSymbol(string symbolName, string specificName = null)
        {
            AddSymbolMapping(symbolName, NameType.DirectiveName, specificName);
        }


        /// <summary>
        /// Verifies added symbol names and generates auto names according to <see cref="nameAutoGenerate"/> setting.
        /// <para>
        /// Tracks used specific names and aliases and throws if name already exists.
        /// Silently ignores auto generated names and aliases if they conflict.
        /// </para>
        /// </summary>
        public void VerifyAndGenerateNames()
        {
            usedTokens.Clear();
            var errors = new List<string>();

            foreach (var nameInfo in symbolMappings.Values.SelectMany(nameInfos => nameInfos))
            {
                switch (nameInfo.Type)
                {
                    case NameType.CommandName:
                    case NameType.CommandAlias:
                    case NameType.CommandShortAlias:
                        if (!nameInfo.IsSpecific && nameAutoGenerate.HasFlag(CliNameAutoGenerate.Commands))
                            nameInfo.Name = FindAutoName(nameInfo);
                        break;
                    case NameType.OptionName:
                    case NameType.OptionAlias:
                    case NameType.OptionShortAlias:
                        if (!nameInfo.IsSpecific && nameAutoGenerate.HasFlag(CliNameAutoGenerate.Options))
                            nameInfo.Name = FindAutoName(nameInfo);
                        break;
                    case NameType.ArgumentName:
                        if (!nameInfo.IsSpecific && nameAutoGenerate.HasFlag(CliNameAutoGenerate.Arguments))
                            nameInfo.Name = FindAutoName(nameInfo);
                        break;
                    case NameType.DirectiveName:
                        if (!nameInfo.IsSpecific && nameAutoGenerate.HasFlag(CliNameAutoGenerate.Directives))
                            nameInfo.Name = FindAutoName(nameInfo);
                        break;
                }

                /*
                    Commands are added with name in ValidTokens so commands can conflict with options without prefix.
                    Note that currently this method is only called for the command itself in the Build method because children commands
                    are not yet known at the time (as CliNamer scoped to Build method, can not add children)

                    Options are added with name in ValidTokens so options without prefix can conflict with commands.

                    Arguments are not added in ValidTokens so they won't conflict with other symbols.
                    But we still need to add used token to different dictionary so that FindAutoName works

                    Directives are added with [] around name in ValidTokens so they can conflict with other symbols with [].
                */

                if (!TryAddToken(nameInfo, out var error))
                    errors.Add(error);
            }

            if (errors.Count > 0)
                throw new Exception($"Name conflicts were found:"
                                    + $"\n{string.Join("\n", errors.Select(e => "- " + e))}");

            foreach (var nameInfos in symbolMappings.Values)
            {
                foreach (var nameInfo in nameInfos.ToArray())
                {
                    NameType shortAliasType;

                    switch (nameInfo.Type)
                    {
                        case NameType.CommandName:
                            if (!shortFormAutoGenerate.HasFlag(CliNameAutoGenerate.Commands))
                                continue;
                            if (nameInfos.Any(n => n.Type == NameType.CommandShortAlias))
                                continue;

                            shortAliasType = NameType.CommandShortAlias;
                            break;
                        case NameType.OptionName:
                            if (!shortFormAutoGenerate.HasFlag(CliNameAutoGenerate.Options))
                                continue;
                            if (nameInfos.Any(n => n.Type == NameType.OptionShortAlias))
                                continue;

                            shortAliasType = NameType.OptionShortAlias;
                            break;
                        default:
                            continue;
                    }

                    var shortAlias = FindAutoShortAlias(nameInfo);
                    if (shortAlias != null)
                    {
                        var shortNameInfo = AddSymbolMapping(nameInfo.SymbolName, shortAliasType, shortAlias);
                        TryAddToken(shortNameInfo, out _);
                    }                    
                }
            }
        }


        /// <summary>
        /// Gets a specific command name for a symbol, or an auto generated one by using current <see cref="nameCasingConvention"/>.
        /// <para>
        /// Auto name is generated only when current <see cref="nameAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Commands"/>.
        /// </para>
        /// </summary>
        public string GetCommandName(string symbolName)
        {
            //As we didn't add a CommandName for root command in AddCommandSymbol, return empty and prevent exception below
            if (parentNamer == null)
                return string.Empty;

            var nameInfos = GetNameInfosForSymbol(symbolName);

            var nameInfo = nameInfos.FirstOrDefault(n => n.Type == NameType.CommandName);

            if (nameInfo == null)
                throw new Exception($"No command mapping was added for symbol name \"{symbolName}\" !");

            return nameInfo.Name;
        }

        /// <summary>
        /// Gets a specific option name for a symbol, or an auto generated one by using current <see cref="nameCasingConvention"/> and <see cref="namePrefixConvention"/>.
        /// <para>
        /// Auto name is generated only when current <see cref="nameAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Options"/>.
        /// </para>
        /// </summary>
        public string GetOptionName(string symbolName)
        {
            var nameInfos = GetNameInfosForSymbol(symbolName);

            var nameInfo = nameInfos.FirstOrDefault(n => n.Type == NameType.OptionName);

            if (nameInfo == null)
                throw new Exception($"No option mapping was added for symbol name \"{symbolName}\" !");

            return nameInfo.Name;
        }

        /// <summary>
        /// Gets a specific argument name for a symbol, or an auto generated one by using current <see cref="nameCasingConvention"/>.
        /// <para>
        /// Auto name is generated only when current <see cref="nameAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Arguments"/>.
        /// </para>
        /// </summary>
        public string GetArgumentName(string symbolName)
        {
            var nameInfos = GetNameInfosForSymbol(symbolName);

            var nameInfo = nameInfos.FirstOrDefault(n => n.Type == NameType.ArgumentName);

            if (nameInfo == null)
                throw new Exception($"No argument mapping was added for symbol name \"{symbolName}\" !");

            return nameInfo.Name;
        }

        /// <summary>
        /// Gets a specific directive name for a symbol, or an auto generated one by using current <see cref="nameCasingConvention"/>.
        /// <para>
        /// Auto name is generated only when current <see cref="nameAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Directives"/>.
        /// </para>
        /// </summary>
        public string GetDirectiveName(string symbolName)
        {
            var nameInfos = GetNameInfosForSymbol(symbolName);

            var nameInfo = nameInfos.FirstOrDefault(n => n.Type == NameType.DirectiveName);

            if (nameInfo == null)
                throw new Exception($"No directive mapping was added for symbol name \"{symbolName}\" !");

            return nameInfo.Name;
        }


        /// <summary>
        /// Gets specific command aliases for a symbol, or auto generated ones by using current <see cref="nameCasingConvention"/>.
        /// <para>
        /// Auto aliases is generated only when current <see cref="nameAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Commands"/>.
        /// </para>
        /// <para>
        /// Auto short form alias is added only when current <see cref="shortFormAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Commands"/>
        /// and it is shorter than command name.
        /// </para>
        /// </summary>
        public IEnumerable<string> GetCommandAliases(string symbolName)
        {
            //As we didn't add a CommandName for root command in AddCommandSymbol, return empty and prevent exception below
            if (parentNamer == null)
                 yield break;

            var nameInfos = GetNameInfosForSymbol(symbolName);

            foreach (var nameInfo in nameInfos)
            {
                if (nameInfo.Type == NameType.CommandAlias
                    || nameInfo.Type == NameType.CommandShortAlias)
                    yield return nameInfo.Name;
            }
        }

        /// <summary>
        /// Gets specific option aliases for a symbol, or auto generated ones by using current <see cref="nameCasingConvention"/>.
        /// <para>
        /// Auto aliases is generated only when current <see cref="nameAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Options"/>.
        /// </para>
        /// <para>
        /// Auto short form alias is added only when current <see cref="shortFormAutoGenerate"/> contains <see cref="CliNameAutoGenerate.Options"/>
        /// and it is shorter than option name.
        /// </para>
        /// </summary>
        public IEnumerable<string> GetOptionAliases(string symbolName)
        {
            var nameInfos = GetNameInfosForSymbol(symbolName);

            foreach (var nameInfo in nameInfos)
            {
                if (nameInfo.Type == NameType.OptionAlias
                    || nameInfo.Type == NameType.OptionShortAlias)
                    yield return nameInfo.Name;
            }
        }


        private class NameInfo : IEquatable<NameInfo>
        {
            public string SymbolName { get; init; }

            public string BaseName { get; init; }

            public string Name { get; set; }

            public NameType Type { get; init; }

            public bool IsSpecific { get; init; }


            public bool Equals(NameInfo other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name, StringComparison.Ordinal);
            }

            public override bool Equals(object obj) => Equals(obj as NameInfo);

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return StringComparer.Ordinal.GetHashCode(Name);
            }
        }

        private enum NameType
        {
            CommandName,

            CommandAlias,

            CommandShortAlias,

            OptionName,

            OptionAlias,

            OptionShortAlias,

            ArgumentName,

            DirectiveName
        }

        private NameInfo AddSymbolMapping(string symbolName, NameType type, string specificName)
        {
            if (symbolName == null)
                throw ExceptionUtil.ParameterNull(nameof(symbolName));
            if (string.IsNullOrWhiteSpace(symbolName))
                throw ExceptionUtil.ParameterEmptyString(nameof(symbolName));

            symbolName = symbolName.Trim();

            if (!symbolMappings.TryGetValue(symbolName, out var nameInfos))
            {
                nameInfos = new HashSet<NameInfo>();
                symbolMappings.Add(symbolName, nameInfos);
            }

            string baseName;
            bool isSpecific;

            if (!string.IsNullOrWhiteSpace(specificName))
            {
                baseName = specificName.Trim();
                isSpecific = true;
            }
            else
            {
                baseName = CliStringUtil.StripSuffixes(symbolName, GetSuffixes(type));
                isSpecific = false;
            }

            var name = AddPrefixIfRequired(baseName, type);

            var nameInfo = new NameInfo
            {
                SymbolName = symbolName,
                BaseName = baseName,
                Name = name,
                Type = type,
                IsSpecific = isSpecific
            };

            nameInfos.Add(nameInfo);

            return nameInfo;
        }

        private HashSet<NameInfo> GetNameInfosForSymbol(string symbolName)
        {
            if (symbolName == null)
                throw ExceptionUtil.ParameterNull(nameof(symbolName));
            if (string.IsNullOrWhiteSpace(symbolName))
                throw ExceptionUtil.ParameterEmptyString(nameof(symbolName));

            symbolName = symbolName.Trim();

            if (!symbolMappings.TryGetValue(symbolName, out var nameInfos))
                throw new Exception($"No mappings were added for symbol name \"{symbolName}\" !");

            return nameInfos;
        }

        private static string[] GetSuffixes(NameType type)
        {
            switch (type)
            {
                case NameType.CommandName:
                case NameType.CommandAlias:
                case NameType.CommandShortAlias:
                    return CommandSuffixes;
                case NameType.OptionName:
                case NameType.OptionAlias:
                case NameType.OptionShortAlias:
                    return OptionSuffixes;
                case NameType.ArgumentName:
                    return ArgumentSuffixes;
                case NameType.DirectiveName:
                    return DirectiveSuffixes;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private string AddPrefixIfRequired(string name, NameType type)
        {
            switch (type)
            {
                case NameType.OptionName:
                case NameType.OptionAlias:
                    return CliStringUtil.AddPrefix(name, namePrefixConvention); //will ignore if already has a prefix
                case NameType.OptionShortAlias:
                    return CliStringUtil.AddPrefix(name, shortFormPrefixConvention); //will ignore if already has a prefix
                default:
                    return name;
            }
        }

        private string FindAutoName(NameInfo nameInfo)
        {
            string firstName = null;

            for (var i = 0; i < 5; i++)
            {
                var currentName = (i == 0)
                    ? nameInfo.BaseName
                    : nameInfo.BaseName + "-" + (i + 1);

                currentName = CliStringUtil.ToCase(currentName, nameCasingConvention);

                currentName = AddPrefixIfRequired(currentName, nameInfo.Type);

                if (i == 0)
                    firstName = currentName;

                if (!IsUsedToken(currentName, nameInfo.Type))
                    return currentName;
            }

            return firstName;
        }

        private string FindAutoShortAlias(NameInfo nameInfo)
        {
            var words = CliStringUtil.SplitWords(nameInfo.BaseName);

            foreach (var word in words)
            {
                var name = CliStringUtil.ToCase(word, nameCasingConvention);

                var firstLetter = name.FirstOrDefault(char.IsLetter);

                if (firstLetter == char.MinValue)
                    continue;

                var shortAlias = firstLetter.ToString();

                if (nameInfo.Type == NameType.OptionName)
                    shortAlias = AddPrefixIfRequired(shortAlias, NameType.OptionShortAlias);

                if (!IsUsedToken(shortAlias, nameInfo.Type))
                    return shortAlias;

                shortAlias = char.IsLower(firstLetter)
                    ? shortAlias.ToUpperInvariant()
                    : shortAlias.ToLowerInvariant();

                if (!IsUsedToken(shortAlias, nameInfo.Type))
                    return shortAlias;
            }

            return null;
        }

        private bool IsUsedToken(string name, NameType type)
        {
            return TryGetUsedToken(name, type, out _, out _);
        }

        private bool TryGetUsedToken(string name, NameType type, out NameInfo existingNameInfo, out bool usedInParent)
        {
            usedInParent = false;

            var token = NormalizeToken(name, type);
            var tokenDictonary = GetTokenDictionary(type);

            if (tokenDictonary.TryGetValue(token, out existingNameInfo))
                return true;

            if (parentNamer != null
                && (type == NameType.CommandName || type == NameType.CommandAlias || type == NameType.CommandShortAlias))
            {
                if (parentNamer.GetTokenDictionary(type).TryGetValue(token, out existingNameInfo))
                {
                    usedInParent = true;
                    return true;
                }
            }

            return false;
        }

        private bool TryGetUsedToken(NameInfo nameInfo, out NameInfo existingNameInfo, out bool usedInParent)
        {
            return TryGetUsedToken(nameInfo.Name, nameInfo.Type, out existingNameInfo, out usedInParent);
        }

        private string NormalizeToken(string name, NameType type)
        {
            return (type == NameType.DirectiveName)
                ? $"[{name}]"
                : name;
        }

        private Dictionary<string, NameInfo> GetTokenDictionary(NameType type)
        {
            var dictionaryType = (type == NameType.ArgumentName)
                ? 1
                : 0;

            if (usedTokens.TryGetValue(dictionaryType, out var tokenDictonary))
                return tokenDictonary;

            tokenDictonary = new Dictionary<string, NameInfo>(StringComparer.Ordinal);

            usedTokens.Add(dictionaryType, tokenDictonary);

            return tokenDictonary;
        }

        private bool TryAddToken(NameInfo nameInfo, out string error)
        {
            error = null;

            if (TryGetUsedToken(nameInfo, out var existingNameInfo, out var usedInParent))
            {
                error = usedInParent
                    ? $"{nameInfo.Type} \"{nameInfo.Name}\" for \"{nameInfo.SymbolName}\" conflicts with parent {existingNameInfo.Type} for \"{existingNameInfo.SymbolName}\" !"
                    : $"{nameInfo.Type} \"{nameInfo.Name}\" for \"{nameInfo.SymbolName}\" conflicts with {existingNameInfo.Type} for \"{existingNameInfo.SymbolName}\" !";

                return false;
            }

            var token = NormalizeToken(nameInfo.Name, nameInfo.Type);
            var tokenDictonary = GetTokenDictionary(nameInfo.Type);

            tokenDictonary.Add(token, nameInfo);

            if (parentNamer != null
                && (nameInfo.Type == NameType.CommandName || nameInfo.Type == NameType.CommandAlias || nameInfo.Type == NameType.CommandShortAlias))
                parentNamer.GetTokenDictionary(nameInfo.Type).Add(token, nameInfo);

            return true;
        }
    }
}
