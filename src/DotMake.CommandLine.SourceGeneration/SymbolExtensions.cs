using Microsoft.CodeAnalysis;
using System.Linq;

namespace DotMake.CommandLine.SourceGeneration
{
    public static class SymbolExtensions
    {
        private static readonly SymbolDisplayFormat ReferenceFormat =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                    SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private static readonly SymbolDisplayFormat CompareFormat =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        /// <summary>
        /// Converts the symbol to a string representation, which is suitable for calling the symbol in code.
        /// <para>
        /// Includes fully qualified types, generic type parameters and members
        /// </para>
        /// </summary>
        public static string ToReferenceString(this ISymbol symbol)
        {
            return symbol.ToDisplayString(ReferenceFormat);
        }

        /// <summary>
        /// Converts the symbol to a string representation, which is suitable for comparing symbol.
        /// <para>
        /// Includes fully qualified types, generic type parameters and members, no special types (short names like string, int)
        /// </para>
        /// </summary>
        public static string ToCompareString(this ISymbol symbol)
        {
            return symbol.ToDisplayString(CompareFormat);
        }

        public static ITypeSymbol GetElementTypeIfEnumerable(this ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
                return arrayType.ElementType;

            if (type.SpecialType == SpecialType.System_String)
                return null;

            if (type is INamedTypeSymbol namedType
                && (namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
                    || namedType.AllInterfaces.Any(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T))
                && namedType.TypeArguments.Length == 1)
            {
                return namedType.TypeArguments[0];
            }

            return null;
        }

        public static ITypeSymbol GetTypeIfNullable(this ITypeSymbol type)
        {
            if ((type is INamedTypeSymbol namedType)
                && namedType.IsGenericType
                && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                return namedType.TypeArguments[0];

            return null;
        }
    }
}
