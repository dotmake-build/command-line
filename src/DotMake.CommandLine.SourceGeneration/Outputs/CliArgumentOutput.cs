using System.Collections.Generic;
using System.Linq;
using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliArgumentOutput : OutputBase
    {
        public const string ArgumentClassName = "Argument";
        public const string ArgumentClassNamespace = "System.CommandLine";
        public const string ArgumentArityClassName = "ArgumentArity";

        public static readonly Dictionary<string, string> PropertyMappings = new()
        {
            //{ nameof(CliArgumentAttribute.Hidden), "IsHidden"},
        };

        public CliArgumentOutput(CliArgumentInput input)
            : base(input)
        {
            Input = input;
        }

        public new CliArgumentInput Input { get; }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varNamer, string varBindingContext)
        {
            sb.AppendLine($"// Argument for '{Input.Symbol.Name}' property");

            using (sb.AppendParamsBlockStart($"var {varName} = new {ArgumentClassNamespace}.{ArgumentClassName}<{Input.Symbol.Type.ToReferenceString()}>"))
            {
                if (Input.AttributeArguments.TryGetValue(nameof(CliArgumentAttribute.Name), out var nameValue))
                    sb.AppendLine($"{varNamer}.GetArgumentName(\"{Input.Symbol.Name}\", \"{nameValue}\")");
                else
                    sb.AppendLine($"{varNamer}.GetArgumentName(\"{Input.Symbol.Name}\")");
            }
            using (sb.AppendBlockStart(null, ";"))
            {
                foreach (var kvp in Input.AttributeArguments)
                {
                    switch (kvp.Key)
                    {
                        case nameof(CliArgumentAttribute.Description):
                        case nameof(CliArgumentAttribute.HelpName):
                        case nameof(CliArgumentAttribute.Hidden):
                            if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                                propertyName = kvp.Key;

                            if (Input.AttributeArguments.TryGetResourceProperty(kvp.Key, out var resourceProperty))
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

            if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.AllowedValues), out var allowedValuesTypedConstant))
                sb.AppendLine($"{ArgumentClassNamespace}.ArgumentValidation.AcceptOnlyFromAmong({varName}, new[] {allowedValuesTypedConstant.ToCSharpString()});");

            if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.ValidationRules), out var validationRulesTypedConstant))
                sb.AppendLine($"DotMake.CommandLine.CliValidationExtensions.AddValidator({varName}, {validationRulesTypedConstant.ToCSharpString()});");

            if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.ValidationPattern), out var validationPatternTypedConstant))
            {
                if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.ValidationMessage), out var validationMessageTypedConstant))
                    sb.AppendLine($"DotMake.CommandLine.CliValidationExtensions.AddValidator({varName}, {validationPatternTypedConstant.ToCSharpString()}, {validationMessageTypedConstant.ToCSharpString()});");
                else
                    sb.AppendLine($"DotMake.CommandLine.CliValidationExtensions.AddValidator({varName}, {validationPatternTypedConstant.ToCSharpString()});");
            }

            //In ArgumentArity.Default, Arity is set to ZeroOrMore for IEnumerable if parent is command,
            //but we want to enforce OneOrMore so that Required is consistent
            if (Input.Required
                && Input.ArgumentParser.ItemType != null //if it's a collection type
                && !Input.AttributeArguments.ContainsKey(nameof(CliArgumentAttribute.Arity)))
                sb.AppendLine($"{varName}.Arity = {ArgumentClassNamespace}.{ArgumentArityClassName}.OneOrMore;");

            if (Input.Parent.HasGetCompletionsInterface)
                //sb.AppendLine($"{varDefaultClass}.AddCompletions(\"{Input.Symbol.Name}\", {varName}.CompletionSources);");
                sb.AppendLine($"{varName}.CompletionSources.Add(completionContext => GetCompletions(\"{Input.Symbol.Name}\", {varBindingContext}, completionContext));");

        }
    }
}
