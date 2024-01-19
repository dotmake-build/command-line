using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration;

public class AttributeResourceArguments
{
    private readonly Dictionary<string, IPropertySymbol> dictionary = new Dictionary<string, IPropertySymbol>();

    public AttributeResourceArguments(AttributeData attributeData, SemanticModel semanticModel)
    {
        var argumentSyntaxList = (attributeData.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax)
            ?.ArgumentList?.Arguments;

        if (argumentSyntaxList == null)
            return;

        foreach (var argumentSyntax in argumentSyntaxList)
        {
            if (argumentSyntax.NameEquals != null
                && argumentSyntax.Expression is InvocationExpressionSyntax invocationExpressionSyntax
                && invocationExpressionSyntax.Expression is IdentifierNameSyntax identifierNameSyntax
                && identifierNameSyntax.Identifier.ValueText == "nameof"
                && invocationExpressionSyntax.ArgumentList.Arguments.Count == 1)
            {
                var nameofArgument = invocationExpressionSyntax.ArgumentList.Arguments[0].Expression;

                if (semanticModel.GetSymbolInfo(nameofArgument).Symbol is IPropertySymbol propertySymbol
                    && propertySymbol.Type.SpecialType == SpecialType.System_String)
                {
                    var classAttributeData = propertySymbol.ContainingType.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass.ToCompareString() == "System.CodeDom.Compiler.GeneratedCodeAttribute");

                    if (classAttributeData != null
                        && classAttributeData.ConstructorArguments.Length > 0
                        && !classAttributeData.ConstructorArguments[0].IsNull
                        && classAttributeData.ConstructorArguments[0].Value?.ToString() == "System.Resources.Tools.StronglyTypedResourceBuilder")
                    {
                        dictionary.Add(
                            argumentSyntax.NameEquals.Name.Identifier.ValueText,
                            propertySymbol
                        );
                    }
                }
            }
        }
    }

    public bool TryGetValue(string argumentName, out IPropertySymbol resourceProperty)
    {
        return dictionary.TryGetValue(argumentName, out resourceProperty);
    }
}
