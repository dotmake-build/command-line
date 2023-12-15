using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliCommandSettings
    {
        private static readonly CliCommandAttribute Defaults = CliCommandAttribute.Default;
        private CliNameCasingConvention? nameCasingConvention;
        private CliNamePrefixConvention? namePrefixConvention;
        private CliNamePrefixConvention? shortFormPrefixConvention;
        private bool? shortFormAutoGenerate;

        public CliCommandSettings(INamedTypeSymbol symbol)
        {
            Symbol = symbol;
        }

        public INamedTypeSymbol Symbol { get; }

        public INamedTypeSymbol ParentSymbol { get; internal set; }

        public CliCommandSettings ParentSettings { get; internal set; }

        public bool IsParentCircular { get; private set; }

        public bool IsParentContaining => (ParentSymbol != null && ParentSymbol.Equals(Symbol.ContainingType, SymbolEqualityComparer.Default));

        public CliNameCasingConvention NameCasingConvention
        {
            get => nameCasingConvention ?? ParentSettings?.NameCasingConvention ?? Defaults.NameCasingConvention;
            private set => nameCasingConvention = value;
        }

        public CliNamePrefixConvention NamePrefixConvention
        {
            get => namePrefixConvention ?? ParentSettings?.NamePrefixConvention ?? Defaults.NamePrefixConvention;
            private set => namePrefixConvention = value;
        }

        public CliNamePrefixConvention ShortFormPrefixConvention
        {
            get => shortFormPrefixConvention ?? ParentSettings?.ShortFormPrefixConvention ?? Defaults.ShortFormPrefixConvention;
            private set => shortFormPrefixConvention = value;
        }

        public bool ShortFormAutoGenerate
        {
            get => shortFormAutoGenerate ?? ParentSettings?.ShortFormAutoGenerate ?? Defaults.ShortFormAutoGenerate;
            private set => shortFormAutoGenerate = value;
        }

        public IEnumerable<CliCommandSettings> GetParentTree()
        {
            var settings = ParentSettings;

            while (settings != null)
            {
                yield return settings;

                settings = settings.ParentSettings;
            }
        }

        public string GetContainingTypeFullName(string classSuffix)
        {
            var parentTree = GetParentTree()
                .Prepend(this) //to include first ParentSymbol
                .TakeWhile(s => s.IsParentContaining)
                .Reverse()
                .Select((s, i) =>
                    ((i == 0) ? s.ParentSymbol.ToDisplayString() : s.ParentSymbol.Name) + classSuffix);

            return string.Join(".", parentTree);
        }

        internal static CliCommandSettings Parse(INamedTypeSymbol symbol, AttributeData attributeData, IDictionary<string, TypedConstant> otherArgumentsToFill)
        {
            var settings = new CliCommandSettings(symbol);

            foreach (var kvp in attributeData.NamedArguments)
            {
                var name = kvp.Key;
                var typedConstant = kvp.Value;

                if (typedConstant.IsNull) //IsNull should be used as Value can throw for arrays
                    continue;

                switch (name)
                {
                    case nameof(CliCommandAttribute.Parent):
                        settings.ParentSymbol = (INamedTypeSymbol)typedConstant.Value;
                        break;
                    case nameof(CliCommandAttribute.NameCasingConvention):
                        if (typedConstant.Value != null) //Used only for casting warning
                            settings.NameCasingConvention = (CliNameCasingConvention)typedConstant.Value;
                        break;
                    case nameof(CliCommandAttribute.NamePrefixConvention):
                        if (typedConstant.Value != null)
                            settings.NamePrefixConvention = (CliNamePrefixConvention)typedConstant.Value;
                        break;
                    case nameof(CliCommandAttribute.ShortFormPrefixConvention):
                        if (typedConstant.Value != null)
                            settings.ShortFormPrefixConvention = (CliNamePrefixConvention)typedConstant.Value;
                        break;
                    case nameof(CliCommandAttribute.ShortFormAutoGenerate):
                        if (typedConstant.Value != null)
                            settings.ShortFormAutoGenerate = (bool)typedConstant.Value;
                        break;
                    default:
                        otherArgumentsToFill?.Add(name, typedConstant);
                        break;
                }
            }

            return settings;
        }

        internal void PopulateParentTree()
        {
            var currentSettings = this;
            var currentSymbol = Symbol;
            var visitedSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

            while (currentSymbol != null)
            {
                visitedSymbols.Add(currentSymbol);

                INamedTypeSymbol currentParentSymbol;
                var parentAttributeData = currentSymbol.ContainingType?.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == CliCommandInfo.AttributeFullName);
                if (parentAttributeData != null)
                    currentParentSymbol = currentSymbol.ContainingType;
                else
                {
                    currentParentSymbol = currentSettings.ParentSymbol;
                    parentAttributeData = currentParentSymbol?.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == CliCommandInfo.AttributeFullName);
                }

                if (parentAttributeData == null) //if currentParentSymbol does not have the attribute, parentSettings will be null.
                    break;

                if (currentParentSymbol != null
                    && visitedSymbols.Contains(currentParentSymbol)) //prevent circular dependency
                {
                    currentSettings.ParentSymbol = currentParentSymbol;
                    currentSettings.IsParentCircular = true;
                    break;
                }

                var parentSettings = Parse(currentParentSymbol, parentAttributeData, null);
                currentSettings.ParentSettings = parentSettings;
                currentSettings.ParentSymbol = currentParentSymbol;

                currentSettings = parentSettings;
                currentSymbol = currentParentSymbol;
            }
        }
    }
}
