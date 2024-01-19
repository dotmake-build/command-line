using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliOptionInfo : CliSymbolInfo, IEquatable<CliOptionInfo>
    {
        public static readonly string AttributeFullName = typeof(CliOptionAttribute).FullName;
        public static readonly string[] Suffixes = CliCommandInfo.Suffixes.Select(s => s + "Option").Append("Option").ToArray();
        public const string OptionClassName = "Option";
        public const string OptionClassNamespace = "System.CommandLine";
        public const string DiagnosticName = "CLI option";
        public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
        {
            { nameof(CliOptionAttribute.HelpName), "ArgumentHelpName"},
            { nameof(CliOptionAttribute.Hidden), "IsHidden"}
        };

        public CliOptionInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;
            Parent = parent;

            ParseInfo = new CliArgumentParseInfo(Symbol, syntaxNode, semanticModel, this);

            Analyze();

            if (HasProblem)
                return;

            AttributeArguments = new AttributeArguments(attributeData);

            if (AttributeArguments.TryGetValue(nameof(CliOptionAttribute.Global), out var globalValue))
                Global = (bool)globalValue;
            if (AttributeArguments.TryGetValue(nameof(CliOptionAttribute.Required), out var requiredValue))
                Required = (bool)requiredValue;
            else
                Required = (SyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax && propertyDeclarationSyntax.Initializer != null)
                               ? propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.NullKeyword)
                                 || propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.SuppressNullableWarningExpression)
                               : Symbol.Type.IsReferenceType || Symbol.IsRequired;
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

        public AttributeArguments AttributeArguments { get; }

        public CliCommandInfo Parent { get; }

        public bool Global { get; }

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

            var optionName = AttributeArguments.TryGetValue(nameof(CliOptionAttribute.Name), out var nameValue)
                                    && !string.IsNullOrWhiteSpace(nameValue.ToString())
                ? nameValue.ToString().Trim()
                : Symbol.Name.StripSuffixes(Suffixes).ToCase(Parent.Settings.NameCasingConvention)
                    .AddPrefix(Parent.Settings.NamePrefixConvention);


            sb.AppendLine($"// Option for '{Symbol.Name}' property");
            using (sb.AppendParamsBlockStart($"var {varName} = new {OptionClassNamespace}.{OptionClassName}<{Symbol.Type.ToReferenceString()}>"))
            {
                sb.AppendLine($"\"{optionName}\",");
                ParseInfo.AppendCSharpCallString(sb);
            }
            using (sb.AppendBlockStart(null, ";"))
            {
                foreach (var kvp in AttributeArguments)
                {
                    switch (kvp.Key)
                    {
                        case nameof(CliOptionAttribute.Description):
                        case nameof(CliOptionAttribute.HelpName):
                        case nameof(CliOptionAttribute.Hidden):
                        case nameof(CliOptionAttribute.AllowMultipleArgumentsPerToken):
                            if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                                propertyName = kvp.Key;

                            if (attributeResourceArguments.TryGetValue(kvp.Key, out var resourceProperty))
                                sb.AppendLine($"{propertyName} = {resourceProperty.ToReferenceString()},");
                            else
                                sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                            break;
                        case nameof(CliOptionAttribute.Arity):
                            var arity = kvp.Value.ToCSharpString().Split('.').Last();
                            sb.AppendLine($"{kvp.Key} = {CliArgumentInfo.ArgumentClassNamespace}.{CliArgumentInfo.ArgumentArityClassName}.{arity},");
                            break;
                    }
                }

                //Required is special as it can be calculated when CliOptionAttribute.Required is missing (not forced)
                sb.AppendLine($"IsRequired = {Required.ToString().ToLowerInvariant()},");
            }

            if (AttributeArguments.TryGetTypedConstant(nameof(CliOptionAttribute.AllowedValues), out var allowedValuesTypedConstant))
                sb.AppendLine($"{OptionClassNamespace}.OptionExtensions.FromAmong({varName}, new[] {allowedValuesTypedConstant.ToCSharpString()});");

            if (AttributeArguments.TryGetValue(nameof(CliOptionAttribute.AllowExisting), out var allowExistingValue)
                && (bool)allowExistingValue)
                sb.AppendLine($"{OptionClassNamespace}.OptionExtensions.ExistingOnly({varName});");

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

            if (AttributeArguments.TryGetValues(nameof(CliOptionAttribute.Aliases), out var aliasesValues))
            {
                foreach (var aliasValue in aliasesValues)
                {
                    var alias = aliasValue?.ToString();
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
