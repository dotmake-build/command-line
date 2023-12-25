using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliArgumentInfo : CliSymbolInfo, IEquatable<CliArgumentInfo>
    {
        public static readonly string AttributeFullName = typeof(CliArgumentAttribute).FullName;
        public const string AttributeNameProperty = nameof(CliArgumentAttribute.Name);
        public const string AttributeRequiredProperty = nameof(CliArgumentAttribute.Required);
        public const string AttributeArityProperty = nameof(CliArgumentAttribute.Arity);
        public const string AttributeAllowedValuesProperty = nameof(CliArgumentAttribute.AllowedValues);
        public static readonly string[] Suffixes = CliCommandInfo.Suffixes.Select(s => s + "Argument").Append("Argument").ToArray();
        public const string ArgumentClassName = "Argument";
        public const string ArgumentClassNamespace = "System.CommandLine";
        public const string ArgumentArityClassName = "ArgumentArity";
        public const string DiagnosticName = "CLI argument";
        public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
        {
            { nameof(CliArgumentAttribute.Hidden), "IsHidden"},
        };
        public static readonly HashSet<string> SupportedConverters = new HashSet<string>
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


        public CliArgumentInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
         : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;
            Parent = parent;

            TypeNeedingConverter = FindTypeIfNeedsConverter(Symbol.Type);
            if (TypeNeedingConverter != null)
                Converter = FindConverter(TypeNeedingConverter);

            Analyze();

            if (HasProblem)
                return;

            AttributeArguments = attributeData.NamedArguments.Where(pair => !pair.Value.IsNull)
                .ToImmutableDictionary(pair => pair.Key, pair => pair.Value);

            if (AttributeArguments.TryGetValue(AttributeRequiredProperty, out var requiredTypedConstant)
                && requiredTypedConstant.Value != null)
                Required = (bool)requiredTypedConstant.Value;
        }

        public CliArgumentInfo(GeneratorAttributeSyntaxContext attributeSyntaxContext)
            : this(attributeSyntaxContext.TargetSymbol,
                attributeSyntaxContext.TargetNode,
                attributeSyntaxContext.Attributes[0],
                attributeSyntaxContext.SemanticModel,
                null)
        {
        }

        public new IPropertySymbol Symbol { get; }

        public ImmutableDictionary<string, TypedConstant> AttributeArguments { get; }

        public CliCommandInfo Parent { get; }

        public bool Required { get; } = CliArgumentAttribute.Default.Required;

        public ITypeSymbol TypeNeedingConverter { get; }

        public IMethodSymbol Converter { get; }

        private void Analyze()
        {
            if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
                || Symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningPropertyNotPublicNonStatic, DiagnosticName);
            else
            {
                if (Symbol.GetMethod == null
                    || (Symbol.GetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.GetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicGetter, DiagnosticName);

                if (Symbol.SetMethod == null
                    || (Symbol.SetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.SetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicSetter, DiagnosticName);

                if (TypeNeedingConverter != null && Converter == null)
                    AddDiagnostic(DiagnosticDescriptors.WarningPropertyTypeIsNotBindable, DiagnosticName, TypeNeedingConverter);
            }
        }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varDefaultValue)
        {
            var argumentName = AttributeArguments.TryGetValue(AttributeNameProperty, out var nameTypedConstant)
                                        && !string.IsNullOrWhiteSpace(nameTypedConstant.Value?.ToString())
                ? nameTypedConstant.Value.ToString().Trim()
                : Symbol.Name.StripSuffixes(Suffixes).ToCase(Parent.Settings.NameCasingConvention);

            sb.AppendLine($"// Argument for '{Symbol.Name}' property");
            using (sb.AppendParamsBlockStart($"var {varName} = new {ArgumentClassNamespace}.{ArgumentClassName}<{Symbol.Type.ToReferenceString()}>"))
            {
                sb.AppendLine($"\"{argumentName}\"");
                if (Converter != null)
                {
                    var parseArgument = $", GetParseArgument<{Symbol.Type.ToReferenceString()}, {Converter.ContainingType.ToReferenceString()}>";
                    if (Converter.Name == ".ctor")
                        sb.AppendLine($"{parseArgument}(input => new {Converter.ContainingType.ToReferenceString()}(input))");
                    else
                        sb.AppendLine($"{parseArgument}(input => {Converter.ToReferenceString()}(input))");
                }
            }
            using (sb.AppendBlockStart(null, ";"))
            {
                foreach (var kvp in AttributeArguments)
                {
                    switch (kvp.Key)
                    {
                        case AttributeNameProperty:
                        case AttributeAllowedValuesProperty:
                        case AttributeRequiredProperty:
                            continue;
                        case AttributeArityProperty:
                            var arity = kvp.Value.ToCSharpString().Split('.').Last();
                            sb.AppendLine($"{kvp.Key} = {ArgumentClassNamespace}.{ArgumentArityClassName}.{arity},");
                            break;
                        default:
                            if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                                propertyName = kvp.Key;

                            sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                            break;
                    }
                }
            }

            if (AttributeArguments.TryGetValue(AttributeAllowedValuesProperty, out var allowedValuesTypedConstant)
                && !allowedValuesTypedConstant.IsNull)
                sb.AppendLine($"{ArgumentClassNamespace}.ArgumentExtensions.FromAmong({varName}, new[] {allowedValuesTypedConstant.ToCSharpString()});");

            if (!Required)
                sb.AppendLine($"{varName}.SetDefaultValue({varDefaultValue});");
        }

        public bool Equals(CliArgumentInfo other)
        {
            return base.Equals(other);
        }

        public static ITypeSymbol FindTypeIfNeedsConverter(ITypeSymbol type)
        {
            while (true)
            {
                // note we want System.String and not string so use MetadataName instead of ToDisplayString or ToReferenceString
                if (type.TypeKind == TypeKind.Enum || SupportedConverters.Contains(type.ToCompareString()))
                    return null;

                var underlyingType = type.GetTypeIfNullable();
                if (underlyingType != null)
                {
                    type = underlyingType;
                    continue;
                }

                var itemType = type.GetElementTypeIfEnumerable();
                if (itemType != null)
                {
                    type = itemType;
                    continue;
                }
                
                return type;
            }
        }

        public static IMethodSymbol FindConverter(ITypeSymbol type)
        {
            // INamedTypeSymbol: Represents a type other than an array, a pointer, a type parameter.
            if (!(type is INamedTypeSymbol namedType))
                return null;
            
            var method = namedType.InstanceConstructors.FirstOrDefault(c =>
                (c.DeclaredAccessibility == Accessibility.Public)
                && c.Parameters.Length > 0
                && c.Parameters[0].Type.SpecialType == SpecialType.System_String
                && c.Parameters.Skip(1).All(p => p.IsOptional)
            );

            if (method == null)
                method = (IMethodSymbol)namedType.GetMembers().FirstOrDefault(s =>
                    s is IMethodSymbol m
                    && (m.DeclaredAccessibility == Accessibility.Public)
                    && m.IsStatic && m.Name == "Parse"
                    && m.Parameters.Length > 0
                    && m.Parameters[0].Type.SpecialType == SpecialType.System_String
                    && m.Parameters.Skip(1).All(p => p.IsOptional
                    && m.ReturnType.Equals(namedType, SymbolEqualityComparer.Default))
                );

            return method;
        }
    }
}
