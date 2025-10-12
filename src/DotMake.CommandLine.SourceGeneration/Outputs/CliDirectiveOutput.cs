using System.Collections.Generic;
using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliDirectiveOutput : OutputBase
    {
        public const string DirectiveClassName = "Directive";

        public static readonly Dictionary<string, string> PropertyMappings = new()
        {
            //{ nameof(CliArgumentAttribute.Hidden), "IsHidden"},
        };

        public CliDirectiveOutput(CliDirectiveInput input)
            : base(input)
        {
            Input = input;
        }

        public new CliDirectiveInput Input { get; }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName, string varNamer)
        {
            sb.AppendLine($"// Directive for '{Input.Symbol.Name}' property");

            using (sb.AppendParamsBlockStart($"var {varName} = new {OutputNamespaces.SystemCommandLine}.{DirectiveClassName}"))
            {
                if (Input.AttributeArguments.TryGetValue(nameof(CliDirectiveAttribute.Name), out var nameValue))
                    sb.AppendLine($"{varNamer}.GetDirectiveName(\"{Input.Symbol.Name}\", \"{nameValue}\")");
                else
                    sb.AppendLine($"{varNamer}.GetDirectiveName(\"{Input.Symbol.Name}\")");
            }
            using (sb.AppendBlockStart(null, ";"))
            {
                foreach (var kvp in Input.AttributeArguments)
                {
                    switch (kvp.Key)
                    {
                        case nameof(CliDirectiveAttribute.Description):
                        case nameof(CliDirectiveAttribute.Hidden):
                            if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                                propertyName = kvp.Key;

                            if (Input.AttributeArguments.TryGetResourceProperty(kvp.Key, out var resourceProperty))
                                sb.AppendLine($"{propertyName} = {resourceProperty.ToReferenceString()},");
                            else
                                sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                            break;
                    }
                }
                /*
                //Required is special as it can be calculated when CliDirectiveAttribute.Required is missing (not forced)
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
                argumentParserOutput.AppendCSharpCallString(sb, "CustomParser", ",");
                */
            }

        }
    }
}
