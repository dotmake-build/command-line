using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliArgumentParserInput : InputBase, IEquatable<CliArgumentParserInput>
    {
        private static readonly HashSet<string> SupportedConverters = new HashSet<string>
        {
            "System.String",
            "System.Boolean",

            "System.IO.FileSystemInfo",
            "System.IO.FileInfo",
            "System.IO.DirectoryInfo",

            "System.Int32",
            "System.Int64",
            "System.Int16",
            "System.UInt32",
            "System.UInt64",
            "System.UInt16",

            "System.Double",
            "System.Single",
            "System.Decimal",

            "System.Byte",
            "System.SByte",

            "System.DateTime",
            "System.DateTimeOffset",
            "System.DateOnly",
            "System.TimeOnly",
            "System.TimeSpan",

            "System.Guid",

            "System.Uri",
            "System.Net.IPAddress",
            "System.Net.IPEndPoint"
        };

        public CliArgumentParserInput(IPropertySymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel, InputBase parent)
            : base(symbol, syntaxNode, semanticModel)
        {
            Parent = parent;

            Type = symbol.Type;
            ItemType = Type.GetElementTypeIfEnumerable(semanticModel.Compilation.GetSpecialType(SpecialType.System_String));
            if (ItemType != null)
            {
                if (NeedsConverter(Type))
                {
                    TypeNeedsConverter = true;
                    Converter = FindEnumerableConverter(Type, ItemType, semanticModel.Compilation);
                }

                if (NeedsConverter(ItemType))
                {
                    ItemTypeNeedsConverter = true;
                    ItemConverter = FindConverter(ItemType);
                }
            }
            else if (NeedsConverter(Type))
            {
                TypeNeedsConverter = true;
                Converter = FindConverter(Type);
            }

            Analyze(symbol);
        }

        public InputBase Parent { get; }

        public ITypeSymbol Type { get; }

        public ITypeSymbol ItemType{ get; }

        public bool TypeNeedsConverter { get; }

        public bool ItemTypeNeedsConverter { get; }

        public IMethodSymbol Converter { get; }

        public IMethodSymbol ItemConverter { get; }

        public sealed override void Analyze(ISymbol symbol)
        {
            var diagnosticName = Parent is CliOptionInput ? CliOptionInput.DiagnosticName : CliArgumentInput.DiagnosticName;

            if (ItemType != null)
            {
                if (TypeNeedsConverter && Converter == null)
                    AddDiagnostic(DiagnosticDescriptors.WarningPropertyTypeEnumerableIsNotBindable, diagnosticName, Type);

                if (ItemTypeNeedsConverter && ItemConverter == null)
                    AddDiagnostic(DiagnosticDescriptors.WarningPropertyTypeIsNotBindable, diagnosticName, ItemType);
            }
            else
            {
                if (TypeNeedsConverter && Converter == null)
                    AddDiagnostic(DiagnosticDescriptors.WarningPropertyTypeIsNotBindable, diagnosticName, Type);
            }
        }

        public bool Equals(CliArgumentParserInput other)
        {
            return base.Equals(other);
        }

        private static bool NeedsConverter(ITypeSymbol type)
        {
            var underlyingType = type.GetUnderlyingTypeIfNullable();
            if (underlyingType != null)
                return NeedsConverter(underlyingType);

            // note we want System.String and not string so use MetadataName instead of ToDisplayString or ToReferenceString
            if (type.TypeKind == TypeKind.Enum || SupportedConverters.Contains(type.ToCompareString()))
                return false;

            return true;
        }
        
        private static IMethodSymbol FindEnumerableConverter(ITypeSymbol type, ITypeSymbol itemType, Compilation compilation)
        {
            var underlyingType = type.GetUnderlyingTypeIfNullable();
            if (underlyingType != null)
                return FindConverter(underlyingType);

            var arrayMethod = compilation.GetSpecialType(SpecialType.System_Array).GetMembers().OfType<IMethodSymbol>().First();

            if (type is IArrayTypeSymbol)
                return arrayMethod;

            // INamedTypeSymbol: Represents a type other than an array, a pointer, a type parameter.
            if (!(type is INamedTypeSymbol namedType))
                return null;

            // If type is interface itself
            if (namedType.SpecialType == SpecialType.System_Collections_IEnumerable
                || namedType.ToCompareString() == "System.Collections.IList"
                || namedType.ToCompareString() == "System.Collections.ICollection"
                || //generic ones:
                namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
                || namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IList_T
                || namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_ICollection_T)
                return arrayMethod;

            //If type inherits IEnumerable<T> and has a constructor with IEnumerable<T> or IList<T> parameter (as in Collection<T>)
            if (namedType.AllInterfaces.Any(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T))
            {
                return namedType.InstanceConstructors.FirstOrDefault(c =>
                    c.DeclaredAccessibility == Accessibility.Public
                    && c.Parameters.Length > 0
                    && c.Parameters.Skip(1).All(p => p.IsOptional)
                    && c.Parameters[0].Type is INamedTypeSymbol parameterType
                    && (parameterType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
                       || parameterType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IList_T)
                    && parameterType.TypeArguments[0].Equals(itemType, SymbolEqualityComparer.Default)
                );
            }

            return null;
        }

        private static IMethodSymbol FindConverter(ITypeSymbol type)
        {
            var underlyingType = type.GetUnderlyingTypeIfNullable();
            if (underlyingType != null)
                return FindConverter(underlyingType);

            // INamedTypeSymbol: Represents a type other than an array, a pointer, a type parameter.
            if (!(type is INamedTypeSymbol namedType))
                return null;

            var method = namedType.InstanceConstructors.FirstOrDefault(c =>
                (c.DeclaredAccessibility == Accessibility.Public)
                && c.Parameters.Length > 0
                && c.Parameters.Skip(1).All(p => p.IsOptional)
                && c.Parameters[0].Type.SpecialType == SpecialType.System_String
            );

            if (method == null)
                method = (IMethodSymbol)namedType.GetMembers().FirstOrDefault(s =>
                    s is IMethodSymbol m
                    && (m.DeclaredAccessibility == Accessibility.Public)
                    && m.IsStatic && m.Name == "Parse"
                    && m.Parameters.Length > 0
                    && m.Parameters.Skip(1).All(p => p.IsOptional
                    && m.Parameters[0].Type.SpecialType == SpecialType.System_String
                    && m.ReturnType.Equals(namedType, SymbolEqualityComparer.Default))
                );

            return method;
        }
    }
}
