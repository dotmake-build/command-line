using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
    public class AttributeArguments : IEnumerable<KeyValuePair<string, TypedConstant>>
    {
        private readonly AttributeData attributeData;
        private readonly ImmutableDictionary<string, TypedConstant> dictionary;

        public AttributeArguments(AttributeData attributeData)
        {
            this.attributeData = attributeData;

            //Filter out arguments with null value to avoid repeated checks
            //Note: IsNull should be used as Value can throw for arrays
            dictionary = attributeData.NamedArguments.Where(pair => !pair.Value.IsNull)
                .ToImmutableDictionary();
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

        public AttributeResourceArguments GetResourceArguments(SemanticModel semanticModel)
        {
            return new AttributeResourceArguments(attributeData, semanticModel);
        }

        public IEnumerator<KeyValuePair<string, TypedConstant>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
