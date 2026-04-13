using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliOptionOutput : OutputBase
    {
        public const string OptionClassName = "Option";

        public static readonly Dictionary<string, string> PropertyMappings = new()
        {
            //{ nameof(CliOptionAttribute.HelpName), "ArgumentHelpName"},
            //{ nameof(CliOptionAttribute.Hidden), "IsHidden"}
        };

        public CliOptionOutput(CliOptionInput input)
            : base(input)
        {
            Input = input;
        }

        public new CliOptionInput Input { get; }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varNamer, string varBindingContext)
        {
            sb.AppendLine($"// Option for '{Input.Symbol.Name}' property");

            using (sb.AppendParamsBlockStart($"var {varName} = new {OutputNamespaces.SystemCommandLine}.{OptionClassName}<{Input.Symbol.Type.ToReferenceString()}>"))
            {
                if (Input.AttributeArguments.TryGetValue(nameof(CliOptionAttribute.Name), out var nameValue))
                    sb.AppendLine($"{varNamer}.GetOptionName(\"{Input.Symbol.Name}\", \"{nameValue}\")");
                else
                    sb.AppendLine($"{varNamer}.GetOptionName(\"{Input.Symbol.Name}\")");
            }
            using (sb.AppendBlockStart(null, ";"))
            {
                foreach (var kvp in Input.AttributeArguments)
                {
                    switch (kvp.Key)
                    {
                        case nameof(CliOptionAttribute.Description):
                        case nameof(CliOptionAttribute.HelpName):
                        case nameof(CliOptionAttribute.Hidden):
                        case nameof(CliOptionAttribute.Recursive):
                        case nameof(CliOptionAttribute.AllowMultipleArgumentsPerToken):
                            if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                                propertyName = kvp.Key;

                            string valueString;
                            if (Input.AttributeArguments.TryGetResourceProperty(kvp.Key, out var resourceProperty))
                                valueString = resourceProperty.ToReferenceString();
                            else
                                valueString = kvp.Value.ToCSharpString();

                            // Append groupName to Description in help output
                            if (kvp.Key == nameof(CliOptionAttribute.Description) && !string.IsNullOrEmpty(Input.GroupName))
                            {
                                var group = Input.GroupName;
                                IReadOnlyList<string> parentRequiredGroups = Input.Parent?.RequiredGroups ?? Array.Empty<string>();
                                var isRequiredGroup = parentRequiredGroups.Any(r => string.Equals(r, group, StringComparison.OrdinalIgnoreCase));
                                var suffixLiteral = isRequiredGroup
                                        ? $"\" [Group: '{group}', required]\""
                                        : $"\" [Group: '{group}']\"";
                                valueString = $"{valueString} + {suffixLiteral}";
                            }

                            sb.AppendLine($"{propertyName} = {valueString},");
                            break;
                        case nameof(CliOptionAttribute.Arity):
                            //Note that ArgumentArity from System.CommandLine is not an enum (a struct)
                            //so we simply use same struct property names in our CliArgumentArity enum
                            //this way we can convert to ArgumentArity by calling the same name on the struct
                            var arityName = EnumUtil<CliArgumentArity>.ToName((CliArgumentArity)(kvp.Value.Value ?? 0));
                            sb.AppendLine($"{kvp.Key} = {OutputNamespaces.SystemCommandLine}.{CliArgumentOutput.ArgumentArityClassName}.{arityName},");
                            break;
                    }
                }

                //Required is special as it can be calculated when CliOptionAttribute.Required is missing (not forced)
                sb.AppendLine($"Required = {Input.Required.ToString().ToLowerInvariant()},");
                if (!Input.Required)
                {
                    //No more using a default instance here to avoid IServiceProvider integration causing unnecessary instantiations.
                    //Instead, we read the property initializer SyntaxNode, qualify symbols and then use that SyntaxNode for DefaultValueFactory.
                    //However, we still need an uninitialized instance for being able to call AddCompletions method, for now.
                    //The uninitialized instance can not be used for property access, as the constructor is skipped, properties will come as null.
                    //sb.AppendLine($"DefaultValueFactory = _ => {varDefaultClass}.{Input.Symbol.Name},");

                    SyntaxNode valueExpression = null;
                    if (Input.SyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax
                        && propertyDeclarationSyntax.Initializer != null)
                    {
                        valueExpression = new QualifiedSyntaxRewriter(Input.SemanticModel)
                            .Visit(propertyDeclarationSyntax.Initializer.Value);
                    }

                    if (valueExpression != null)
                        sb.AppendLine($"DefaultValueFactory = _ => {valueExpression},");
                }

                var argumentParserOutput = new CliArgumentParserOutput(Input.ArgumentParser);
                argumentParserOutput.AppendCSharpCallString(sb, "CustomParser", varBindingContext, ",");
            }

            if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliOptionAttribute.AllowedValues), out var allowedValuesTypedConstant))
                sb.AppendLine($"{varName}.AcceptOnlyFromAmong(new[] {allowedValuesTypedConstant.ToCSharpString()});");

            if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliOptionAttribute.ValidationRules), out var validationRulesTypedConstant))
                sb.AppendLine($"{OutputNamespaces.DotMakeCommandLine}.CliValidationExtensions.AddValidator({varName}, {EnumUtil<CliValidationRules>.ToFullName((CliValidationRules)(validationRulesTypedConstant.Value ?? 0))});");

            if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliOptionAttribute.ValidationPattern), out var validationPatternTypedConstant))
            {
                if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliOptionAttribute.ValidationMessage), out var validationMessageTypedConstant))
                    sb.AppendLine($"{OutputNamespaces.DotMakeCommandLine}.CliValidationExtensions.AddValidator({varName}, {validationPatternTypedConstant.ToCSharpString()}, {validationMessageTypedConstant.ToCSharpString()});");
                else
                    sb.AppendLine($"{OutputNamespaces.DotMakeCommandLine}.CliValidationExtensions.AddValidator({varName}, {validationPatternTypedConstant.ToCSharpString()});");
            }

            if (Input.AttributeArguments.TryGetValue(nameof(CliOptionAttribute.Alias), out var aliasValue))
                sb.AppendLine($"{varNamer}.AddShortFormAlias({varName}, \"{Input.Symbol.Name}\", \"{aliasValue}\");");
            else
                sb.AppendLine($"{varNamer}.AddShortFormAlias({varName}, \"{Input.Symbol.Name}\");");

            if (Input.AttributeArguments.TryGetValues(nameof(CliOptionAttribute.Aliases), out var aliasesValues))
            {
                foreach (string alias in aliasesValues)
                    sb.AppendLine($"{varNamer}.AddAlias({varName}, \"{Input.Symbol.Name}\", \"{alias}\");");
            }

            if (Input.Parent.HasGetCompletionsInterface)
                //sb.AppendLine($"{varDefaultClass}.AddCompletions(\"{Input.Symbol.Name}\", {varName}.CompletionSources);");
                sb.AppendLine($"{varName}.CompletionSources.Add(completionContext => GetCompletions(\"{Input.Symbol.Name}\", {varBindingContext}, completionContext));");
        }
    }
}
