// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#nullable enable
// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.CommandLine.Parsing;
using static System.CommandLine.Binding.ArgumentConversionResult;

namespace System.CommandLine.Binding
{
    internal static partial class ArgumentConverter
    {
        internal static ArgumentConversionResult ConvertObject(
            Argument argument,
            Type type,
            object? value,
            LocalizationResources localizationResources)
        {
            switch (value)
            {
                case Token singleValue:
                    return ConvertToken(argument, type, singleValue, localizationResources);

                case IReadOnlyList<Token> manyValues:
                    return ConvertTokens(argument, type, manyValues, localizationResources);

                default:
                    return None(argument);
            }
        }

        private static ArgumentConversionResult ConvertToken(
            Argument argument,
            Type type,
            Token token,
            LocalizationResources localizationResources)
        {
            var value = token.Value;

            if (type.TryGetNullableType(out var nullableType))
            {
                return ConvertToken(argument, nullableType, token, localizationResources);
            }

            if (StringConverters.TryGetValue(type, out var tryConvert))
            {
                if (tryConvert(value, out var converted))
                {
                    return Success(argument, converted);
                }
                else
                {
                    return Failure(argument, type, value, localizationResources);
                }
            }

            if (type.IsEnum)
            {
#if NET6_0_OR_GREATER
                if (Enum.TryParse(type, value, ignoreCase: true, out var converted))
                {
                    return Success(argument, converted);
                }
#else
                try
                {
                    return Success(argument, Enum.Parse(type, value, true));
                }
                catch (ArgumentException)
                {
                }
#endif
            }

            return Failure(argument, type, value, localizationResources);
        }

        private static ArgumentConversionResult ConvertTokens(
            Argument argument,
            Type type,
            IReadOnlyList<Token> tokens,
            LocalizationResources localizationResources,
            ArgumentResult? argumentResult = null)
        {
            var itemType = type.GetElementTypeIfEnumerable() ?? typeof(string);
            var values = CreateEnumerable(type, itemType, tokens.Count);
            var isArray = values is Array;

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                var result = ConvertToken(argument, itemType, token, localizationResources);

                switch (result.Result)
                {
                    case ArgumentConversionResultType.Successful:
                        if (isArray)
                        {
                            values[i] = result.Value;
                        }
                        else
                        {
                            values.Add(result.Value);
                        }

                        break;

                    default: // failures
                        if (argumentResult is { Parent: CommandResult })
                        {
                            argumentResult.OnlyTake(i);

                            i = tokens.Count;
                            break;
                        }

                        return result;
                }
            }

            return Success(argument, values);
        }

        internal static TryConvertArgument? GetConverter(Argument argument)
        {
            if (argument.Arity is { MaximumNumberOfValues: 1, MinimumNumberOfValues: 1 })
            {
                if (argument.ValueType.TryGetNullableType(out var nullableType) &&
                    StringConverters.TryGetValue(nullableType, out var convertNullable))
                {
                    return (ArgumentResult result, out object? value) => ConvertSingleString(result, convertNullable, out value);
                }

                if (StringConverters.TryGetValue(argument.ValueType, out var convert1))
                {
                    return (ArgumentResult result, out object? value) => ConvertSingleString(result, convert1, out value);
                }

                static bool ConvertSingleString(ArgumentResult result, TryConvertString convert, out object? value) =>
                    convert(result.Tokens[result.Tokens.Count - 1].Value, out value);
            }

            if (argument.ValueType.CanBeBoundFromScalarValue())
            {
                return TryConvertArgument;
            }

            return default;
        }

        private static bool CanBeBoundFromScalarValue(this Type type)
        {
            while (true)
            {
                if (type.IsEnum || StringConverters.ContainsKey(type))
                    return true;

                if (type.TryGetNullableType(out var underlyingType))
                {
                    type = underlyingType;
                    continue;
                }

                if (type.GetElementTypeIfEnumerable() is { } itemType)
                {
                    type = itemType;
                    continue;
                }

                return false;
            }
        }

        private static ArgumentConversionResult Failure(
            Argument argument,
            Type expectedType,
            string value,
            LocalizationResources localizationResources)
        {
            return new ArgumentConversionResult(argument, expectedType, value, localizationResources);
        }

        public static bool TryConvertArgument(ArgumentResult argumentResult, out object? value)
        {
            var argument = argumentResult.Argument;

            ArgumentConversionResult result = argument.Arity.MaximumNumberOfValues switch
            {
                // 0 is an implicit bool, i.e. a "flag"
                0 => Success(argumentResult.Argument, true),
                1 => ConvertObject(argument,
                                   argument.ValueType,
                                   argumentResult.Tokens.Count > 0
                                       ? argumentResult.Tokens[argumentResult.Tokens.Count - 1]
                                       : null, 
                                   argumentResult.LocalizationResources),
                _ => ConvertTokens(argument,
                                    argument.ValueType,
                                    argumentResult.Tokens,
                                    argumentResult.LocalizationResources,
                                    argumentResult)
            };

            value = result.Value;
            return result.Result == ArgumentConversionResultType.Successful;
        }
    }
}
