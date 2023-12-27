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
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
                    //SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        private static readonly SymbolDisplayFormat CompareFormat =
            new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

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

        /// <param name="type"></param>
        /// <param name="nonGenericElementType">The element type to use for non-generic IEnumerable interfaces like IList (instead of object).</param>
        public static ITypeSymbol GetElementTypeIfEnumerable(this ITypeSymbol type, ITypeSymbol nonGenericElementType)
        {
            //not common but just in case if it's wrapped in Nullable<T> (struct IEnumerable<T> ?)
            var underlyingType = type.GetUnderlyingTypeIfNullable();
            if (underlyingType != null)
                return underlyingType.GetElementTypeIfEnumerable(nonGenericElementType);

            if (type is IArrayTypeSymbol arrayType)
                return arrayType.ElementType;

            if (type.SpecialType == SpecialType.System_String)
                return null;

            if (type is INamedTypeSymbol namedType)
            {
                // If type is IEnumerable<T> itself
                if (namedType.TypeArguments.Length == 1
                    && namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                    return namedType.TypeArguments[0];

                // If type implements/extends IEnumerable<T>
                var enumerableType = namedType.AllInterfaces
                    .Where(i => i.TypeArguments.Length == 1
                                  && i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                    .Select(i => i.TypeArguments[0])
                    .FirstOrDefault();
                if (enumerableType != null)
                    return enumerableType;
                
                // If type is IEnumerable itself or inherits (IEnumerable -> ICollection -> IList)
                if (namedType.SpecialType == SpecialType.System_Collections_IEnumerable
                    || namedType.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_Collections_IEnumerable))
                    return nonGenericElementType;
            }

            return null;
        }

        public static ITypeSymbol GetUnderlyingTypeIfNullable(this ITypeSymbol type)
        {
            if (type is INamedTypeSymbol namedType
                && namedType.IsGenericType
                && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
                return namedType.TypeArguments[0];

            return null;
        }
    }
}
