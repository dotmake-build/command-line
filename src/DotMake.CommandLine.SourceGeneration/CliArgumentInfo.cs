using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliArgumentInfo : CliSymbolInfo, IEquatable<CliArgumentInfo>
    {
        public static readonly string AttributeFullName = typeof(CliArgumentAttribute).FullName;
        public static readonly string[] Suffixes = CliCommandInfo.Suffixes.Select(s => s + "Argument").Append("Argument").ToArray();
        public const string ArgumentClassName = "CliArgument";
        public const string ArgumentClassNamespace = "System.CommandLine";
        public const string ArgumentArityClassName = "ArgumentArity";
        public const string DiagnosticName = "CLI argument";
        public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
        {
            //{ nameof(CliArgumentAttribute.Hidden), "IsHidden"},
        };

        public CliArgumentInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
         : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;
            Parent = parent;

            ParseInfo = new CliArgumentParseInfo(Symbol, syntaxNode, semanticModel, this);

            Analyze();

            if (HasProblem)
                return;

            AttributeArguments = new AttributeArguments(attributeData);

            if (AttributeArguments.TryGetValue(nameof(CliArgumentAttribute.Required), out var requiredValue))
                Required = (bool)requiredValue;
            else
                Required = (SyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax && propertyDeclarationSyntax.Initializer != null)
                    ? propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.NullKeyword)
                      || propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.SuppressNullableWarningExpression)
                    : Symbol.Type.IsReferenceType || Symbol.IsRequired;
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

        public AttributeArguments AttributeArguments { get; }

        public CliCommandInfo Parent { get; }

        public bool Required { get; }

        public CliArgumentParseInfo ParseInfo { get; set; }

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
            }
        }

        public override void ReportDiagnostics(SourceProductionContext sourceProductionContext)
        {
            base.ReportDiagnostics(sourceProductionContext); //self

            ParseInfo.ReportDiagnostics(sourceProductionContext);
        }
        
        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varDefaultValue)
        {
            var attributeResourceArguments = AttributeArguments.GetResourceArguments(SemanticModel);

            var argumentName = AttributeArguments.TryGetValue(nameof(CliArgumentAttribute.Name), out var nameValue)
                                        && !string.IsNullOrWhiteSpace(nameValue.ToString())
                ? nameValue.ToString().Trim()
                : Symbol.Name.StripSuffixes(Suffixes).ToCase(Parent.Settings.NameCasingConvention);

            sb.AppendLine($"// Argument for '{Symbol.Name}' property");
            using (sb.AppendParamsBlockStart($"var {varName} = new {ArgumentClassNamespace}.{ArgumentClassName}<{Symbol.Type.ToReferenceString()}>"))
            {
                sb.AppendLine($"\"{argumentName}\"");
            }
            using (sb.AppendBlockStart(null, ";"))
            {
                foreach (var kvp in AttributeArguments)
                {
                    switch (kvp.Key)
                    {
                        case nameof(CliArgumentAttribute.Description):
                        case nameof(CliArgumentAttribute.HelpName):
                        case nameof(CliArgumentAttribute.Hidden):
                            if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                                propertyName = kvp.Key;

                            if (attributeResourceArguments.TryGetValue(kvp.Key, out var resourceProperty))
                                sb.AppendLine($"{propertyName} = {resourceProperty.ToReferenceString()},");
                            else
                                sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                            break;
                        case nameof(CliArgumentAttribute.Arity):
                            var arity = kvp.Value.ToCSharpString().Split('.').Last();
                            sb.AppendLine($"{kvp.Key} = {ArgumentClassNamespace}.{ArgumentArityClassName}.{arity},");
                            break;
                    }
                }
            }

            ParseInfo.AppendCSharpCallString(sb, $"{varName}.CustomParser");

            if (AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.AllowedValues), out var allowedValuesTypedConstant))
                sb.AppendLine($"{varName}.AcceptOnlyFromAmong(new[] {allowedValuesTypedConstant.ToCSharpString()});");

            if (AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.ValidationRules), out var validationRulesTypedConstant))
                sb.AppendLine($"DotMake.CommandLine.CliValidationExtensions.AddValidator({varName}, {validationRulesTypedConstant.ToCSharpString()});");

            if (AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.ValidationPattern), out var validationPatternTypedConstant))
            {
                if (AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.ValidationMessage), out var validationMessageTypedConstant))
                    sb.AppendLine($"DotMake.CommandLine.CliValidationExtensions.AddValidator({varName}, {validationPatternTypedConstant.ToCSharpString()}, {validationMessageTypedConstant.ToCSharpString()});");
                else
                    sb.AppendLine($"DotMake.CommandLine.CliValidationExtensions.AddValidator({varName}, {validationPatternTypedConstant.ToCSharpString()});");
            }

            if (!Required)
                sb.AppendLine($"{varName}.DefaultValueFactory = _ => {varDefaultValue};");

            //In ArgumentArity.Default, Arity is set to ZeroOrMore for IEnumerable if parent is command,
            //but we want to enforce OneOrMore so that Required is consistent
            if (Required
                && ParseInfo.ItemType != null //if it's a collection type
                && !AttributeArguments.ContainsKey(nameof(CliArgumentAttribute.Arity)))
                sb.AppendLine($"{varName}.Arity = {ArgumentClassNamespace}.{ArgumentArityClassName}.OneOrMore;");
        }

        public bool Equals(CliArgumentInfo other)
        {
            return base.Equals(other);
        }

    }
}
