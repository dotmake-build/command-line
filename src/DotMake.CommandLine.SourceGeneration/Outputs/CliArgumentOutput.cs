using System.Collections.Generic;
using System.Linq;
using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliArgumentOutput : OutputBase
    {
        public const string ArgumentClassName = "Argument";
        public const string ArgumentClassNamespace = "System.CommandLine";
        public const string ArgumentArityClassName = "ArgumentArity";

        public static readonly string[] Suffixes = CliCommandOutput.Suffixes
            .Select(s => s + "Argument")
            .Append("Argument")
            .ToArray();
        public static readonly Dictionary<string, string> PropertyMappings = new()
        {
            //{ nameof(CliArgumentAttribute.Hidden), "IsHidden"},
        };

        public CliArgumentOutput(CliArgumentInput input)
            : base(input)
        {
            Input = input;
        }

        public new CliArgumentInput Input { get; set; }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varDefaultValue)
        {
            var argumentName = Input.AttributeArguments.TryGetValue(nameof(CliArgumentAttribute.Name), out var nameValue)
                               && !string.IsNullOrWhiteSpace(nameValue.ToString())
                ? $"\"{nameValue.ToString().Trim()}\""
                : $"GetArgumentName(\"{Input.Symbol.Name.StripSuffixes(Suffixes)}\")";

            sb.AppendLine($"// Argument for '{Input.Symbol.Name}' property");
            using (sb.AppendParamsBlockStart($"var {varName} = new {ArgumentClassNamespace}.{ArgumentClassName}<{Input.Symbol.Type.ToReferenceString()}>"))
            {
                sb.AppendLine($"{argumentName}");
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
                    sb.AppendLine($"DefaultValueFactory = _ => {varDefaultValue},");

                var argumentParserOutput = new CliArgumentParserOutput(Input.ArgumentParser);
                argumentParserOutput.AppendCSharpCallString(sb, "CustomParser", ",");
            }

            if (Input.AttributeArguments.TryGetTypedConstant(nameof(CliArgumentAttribute.AllowedValues), out var allowedValuesTypedConstant))
                sb.AppendLine($"{varName}.AcceptOnlyFromAmong(new[] {allowedValuesTypedConstant.ToCSharpString()});");

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
        }
    }
}
