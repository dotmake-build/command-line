using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliOptionInfo : CliSymbolInfo, IEquatable<CliOptionInfo>
    {
        public static readonly string AttributeFullName = typeof(CliOptionAttribute).FullName;
        public const string AttributeNameProperty = nameof(CliOptionAttribute.Name);
        public const string AttributeAliasesProperty = nameof(CliOptionAttribute.Aliases);
        public const string AttributeGlobalProperty = nameof(CliOptionAttribute.Global);
        public const string AttributeRequiredProperty = nameof(CliOptionAttribute.Required);
        public const string AttributeArityProperty = nameof(CliOptionAttribute.Arity);
        public const string AttributeAllowedValuesProperty = nameof(CliOptionAttribute.AllowedValues);
        public static readonly string[] Suffixes = CliCommandInfo.Suffixes.Select(s => s + "Option").Append("Option").ToArray();
        public const string OptionClassName = "Option";
        public const string OptionClassNamespace = "System.CommandLine";
        public const string DiagnosticName = "CLI option";
        public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
        {
            { nameof(CliOptionAttribute.HelpName), "ArgumentHelpName"},
            { nameof(CliOptionAttribute.Hidden), "IsHidden"},
            { nameof(CliOptionAttribute.Required), "IsRequired"}
        };

        public CliOptionInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;
            Parent = parent;

            TypeNeedingConverter = CliArgumentInfo.FindTypeIfNeedsConverter(Symbol.Type);
            if (TypeNeedingConverter != null)
                Converter = CliArgumentInfo.FindConverter(TypeNeedingConverter);

            Analyze();

            if (HasProblem)
                return;

            AttributeArguments = attributeData.NamedArguments.Where(pair => !pair.Value.IsNull)
                .ToImmutableDictionary(pair => pair.Key, pair => pair.Value);

            if (AttributeArguments.TryGetValue(AttributeGlobalProperty, out var globalTypedConstant)
                && globalTypedConstant.Value != null)
                Global = (bool)globalTypedConstant.Value;
            if (AttributeArguments.TryGetValue(AttributeRequiredProperty, out var requiredTypedConstant)
                && requiredTypedConstant.Value != null)
                Required = (bool)requiredTypedConstant.Value;
        }

        public CliOptionInfo(GeneratorAttributeSyntaxContext attributeSyntaxContext)
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

        public bool Global { get; }

        public bool Required { get; }

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
            var optionName = AttributeArguments.TryGetValue(AttributeNameProperty, out var nameTypedConstant)
                                    && !string.IsNullOrWhiteSpace(nameTypedConstant.Value?.ToString())
                ? nameTypedConstant.Value.ToString().Trim()
                : Symbol.Name.StripSuffixes(Suffixes).ToCase(Parent.Settings.NameCasingConvention)
                    .AddPrefix(Parent.Settings.NamePrefixConvention);

            sb.AppendLine($"// Option for '{Symbol.Name}' property");
            using (sb.AppendParamsBlockStart($"var {varName} = new {OptionClassNamespace}.{OptionClassName}<{Symbol.Type.ToReferenceString()}>"))
            {
                sb.AppendLine($"\"{optionName}\"");
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
                        case AttributeAliasesProperty:
                        case AttributeGlobalProperty:
                        case AttributeAllowedValuesProperty:
                            continue;
                        case AttributeArityProperty:
                            var arity = kvp.Value.ToCSharpString().Split('.').Last();
                            sb.AppendLine($"{kvp.Key} = {CliArgumentInfo.ArgumentClassNamespace}.{CliArgumentInfo.ArgumentArityClassName}.{arity},");
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
                sb.AppendLine($"{OptionClassNamespace}.OptionExtensions.FromAmong({varName}, new[] {allowedValuesTypedConstant.ToCSharpString()});");

            if (!Required)
                sb.AppendLine($"{varName}.SetDefaultValue({varDefaultValue});");

            var shortForm = optionName.RemovePrefix();
            if (Parent.Settings.ShortFormAutoGenerate && shortForm.Length >= 2)
            {
                shortForm = shortForm[0].ToString()
                    .AddPrefix(Parent.Settings.ShortFormPrefixConvention);
                if (!Parent.UsedAliases.Contains(shortForm))
                {
                    sb.AppendLine($"{varName}.AddAlias(\"{shortForm}\");");
                    Parent.UsedAliases.Add(shortForm);
                }
            }

            if (AttributeArguments.TryGetValue(AttributeAliasesProperty, out var aliasesTypedConstant)
                && !aliasesTypedConstant.IsNull)
            {
                foreach (var aliasTypedConstant in aliasesTypedConstant.Values)
                {
                    var alias = aliasTypedConstant.Value?.ToString();
                    if (!Parent.UsedAliases.Contains(alias))
                    {
                        sb.AppendLine($"{varName}.AddAlias(\"{alias}\");");
                        Parent.UsedAliases.Add(alias);
                    }
                }
            }
        }

        public bool Equals(CliOptionInfo other)
        {
            return base.Equals(other);
        }
    }
}
