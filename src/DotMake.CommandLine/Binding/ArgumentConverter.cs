// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#nullable enable

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using static DotMake.CommandLine.Binding.ArgumentConversionResult;

namespace DotMake.CommandLine.Binding
{
    internal static partial class ArgumentConverter
    {
        internal static Dictionary<Type, Func<Array, object>> CollectionConverters = new Dictionary<Type, Func<Array, object>>();

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
            var itemType = type.GetElementTypeIfEnumerable(typeof(string)) ?? typeof(string);
            var values = Array.CreateInstance(itemType, tokens.Count); //typed array

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                var result = ConvertToken(argument, itemType, token, localizationResources);

                if (result.Result == ArgumentConversionResultType.Successful)
                {
                    values.SetValue(result.Value, i);
                }
                else
                {
                    // failures
                    if (argumentResult is { Parent: CommandResult })
                    {
                        argumentResult.OnlyTake(i);

                        //i = tokens.Count;
                    }

                    return result;
                }
            }

            if (CollectionConverters.TryGetValue(type, out var convertFromArray))
                return Success(argument, convertFromArray(values));

            return Failure(argument, type, string.Join("|", tokens), localizationResources);
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
            if (type.IsEnum || StringConverters.ContainsKey(type))
                return true;

            if (CollectionConverters.ContainsKey(type))
                return true;

            return false;
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

        internal static object? GetDefaultValue(Type type)
        {
            if (type.IsNullable())
            {
                return null;
            }

            try
            {
                var itemType = type.GetElementTypeIfEnumerable(typeof(string));
                if (itemType != null
                    && CollectionConverters.TryGetValue(type, out var convertFromArray))
                    return convertFromArray(Array.CreateInstance(itemType, 0));
            }
            catch
            {
                return null;
            }

            if (type.IsValueType)
                return CreateDefaultValueType(type);

            return null;
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067:UnrecognizedReflectionPattern",
            Justification = $"{nameof(CreateDefaultValueType)} is only called on a ValueType. You can always create an instance of a ValueType.")]
        private static object CreateDefaultValueType(Type type) =>
            FormatterServices.GetUninitializedObject(type);
    }
}
