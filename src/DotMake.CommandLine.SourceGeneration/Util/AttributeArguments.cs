using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration.Util
{
    public class AttributeArguments : IEnumerable<KeyValuePair<string, TypedConstant>>
    {
        private readonly AttributeData attributeData;
        private readonly ImmutableDictionary<string, TypedConstant> dictionary;
        private readonly Dictionary<string, IPropertySymbol> resourceArguments;

        public AttributeArguments(AttributeData attributeData, SemanticModel semanticModel)
        {
            this.attributeData = attributeData;

            //Filter out arguments with null value to avoid repeated checks
            //Note: IsNull should be used as Value can throw for arrays
            dictionary = attributeData.NamedArguments.Where(pair => !pair.Value.IsNull)
                .ToImmutableDictionary();

            resourceArguments = GetResourceArguments(attributeData, semanticModel);
        }

        public bool ContainsKey(string argumentName)
        {
            return dictionary.ContainsKey(argumentName);
        }

        public bool TryGetValue(string argumentName, out object argumentValue)
        {
            if (dictionary.TryGetValue(argumentName, out var argumentTypedConstant))
            {
                argumentValue = argumentTypedConstant.Value;

                return true;
            }

            argumentValue = null;
            return false;
        }

        public bool TryGetValues(string argumentName, out object[] argumentValues)
        {
            //For array values

            if (dictionary.TryGetValue(argumentName, out var argumentTypedConstant))
            {
                argumentValues = argumentTypedConstant.Values
                    .Where(elementTypeConstant => !elementTypeConstant.IsNull) //skip null elements
                    .Select(elementTypeConstant => elementTypeConstant.Value)
                    .ToArray();

                return true;
            }

            argumentValues = null;
            return false;
        }

        public bool TryGetTypedConstant(string argumentName, out TypedConstant argumentTypedConstant)
        {
            return dictionary.TryGetValue(argumentName, out argumentTypedConstant);
        }

        public bool TryGetResourceProperty(string argumentName, out IPropertySymbol resourceProperty)
        {
            return resourceArguments.TryGetValue(argumentName, out resourceProperty);
        }

        public IEnumerator<KeyValuePair<string, TypedConstant>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static Dictionary<string, IPropertySymbol> GetResourceArguments(AttributeData attributeData, SemanticModel semanticModel)
        {
            var resourceArguments = new Dictionary<string, IPropertySymbol>();

            var argumentSyntaxList = (attributeData.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax)
                ?.ArgumentList?.Arguments;

            if (argumentSyntaxList == null || semanticModel == null)
                return resourceArguments;

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
                            resourceArguments.Add(
                                argumentSyntax.NameEquals.Name.Identifier.ValueText,
                                propertySymbol
                            );
                        }
                    }
                }
            }

            return resourceArguments;
        }
    }
}
