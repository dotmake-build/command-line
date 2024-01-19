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
                    ((i == 0) ? s.ParentSymbol.ToReferenceString() : s.ParentSymbol.Name) + classSuffix);

            return string.Join(".", parentTree);
        }

        internal static CliCommandSettings Parse(INamedTypeSymbol symbol, AttributeArguments attributeArguments)
        {
            var settings = new CliCommandSettings(symbol);

            if (attributeArguments.TryGetValue(nameof(CliCommandAttribute.Parent), out var parentValue))
                settings.ParentSymbol = (INamedTypeSymbol)parentValue;
            if (attributeArguments.TryGetValue(nameof(CliCommandAttribute.NameCasingConvention), out var nameCasingValue))
                settings.NameCasingConvention = (CliNameCasingConvention)nameCasingValue;
            if (attributeArguments.TryGetValue(nameof(CliCommandAttribute.NamePrefixConvention), out var namePrefixValue))
                settings.NamePrefixConvention = (CliNamePrefixConvention)namePrefixValue;
            if (attributeArguments.TryGetValue(nameof(CliCommandAttribute.ShortFormPrefixConvention), out var shortFormPrefixValue))
                settings.ShortFormPrefixConvention = (CliNamePrefixConvention)shortFormPrefixValue;
            if (attributeArguments.TryGetValue(nameof(CliCommandAttribute.ShortFormAutoGenerate), out var shortFormAutoGenerateArgumentValue))
                settings.ShortFormAutoGenerate = (bool)shortFormAutoGenerateArgumentValue;

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
                    .FirstOrDefault(a => a.AttributeClass?.ToCompareString() == CliCommandInfo.AttributeFullName);
                if (parentAttributeData != null)
                    currentParentSymbol = currentSymbol.ContainingType;
                else
                {
                    currentParentSymbol = currentSettings.ParentSymbol;
                    parentAttributeData = currentParentSymbol?.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass?.ToCompareString() == CliCommandInfo.AttributeFullName);
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

                var parentSettings = Parse(currentParentSymbol, new AttributeArguments(parentAttributeData));
                currentSettings.ParentSettings = parentSettings;
                currentSettings.ParentSymbol = currentParentSymbol;

                currentSettings = parentSettings;
                currentSymbol = currentParentSymbol;
            }
        }
    }
}
