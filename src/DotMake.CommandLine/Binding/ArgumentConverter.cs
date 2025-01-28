// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#nullable enable

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static DotMake.CommandLine.Binding.ArgumentConversionResult;

namespace DotMake.CommandLine.Binding
{
    internal static partial class ArgumentConverter
    {
        internal static Dictionary<Type, Func<Array, object>> CollectionConverters = new Dictionary<Type, Func<Array, object>>();

        internal static TryConvertArgument? GetConverter(Argument argument)
        {
            /*
            //We check exceptions for convert methods, so disabling this shortcut
            if (argument.Arity is { MaximumNumberOfValues: 1, MinimumNumberOfValues: 1 })
            {
                var type = argument.ValueType.GetNullableUnderlyingTypeOrSelf();

                if (StringConverters.TryGetValue(type, out var tryConvertString))
                {
                    return (ArgumentResult result, out object? value) => tryConvertString(result.Tokens[result.Tokens.Count - 1].Value, out value);
                }
            }
            */

            if (argument.ValueType.CanBeBoundFromScalarValue())
            {
                return TryConvertArgument;
            }

            return default;
        }

        private static bool CanBeBoundFromScalarValue(this Type type)
        {
            type = type.GetNullableUnderlyingTypeOrSelf();

            if (type.IsEnum || StringConverters.ContainsKey(type))
                return true;

            if (CollectionConverters.ContainsKey(type))
                return true;

            return false;
        }

        internal static bool TryConvertArgument(ArgumentResult argumentResult, out object? value)
        {
            var argument = argumentResult.Argument;

            ArgumentConversionResult result = argument.Arity.MaximumNumberOfValues switch
            {
                // 0 is an implicit bool, i.e. a "flag"
                0 => Success(argumentResult, true),
                1 => ConvertObject(argumentResult,
                    argument.ValueType,
                    argumentResult.Tokens.Count > 0
                        ? argumentResult.Tokens[argumentResult.Tokens.Count - 1]
                        : null),
                _ => ConvertTokens(argumentResult,
                    argument.ValueType,
                    argumentResult.Tokens)
            };

            /*MODIFY*/
            //just return the value as we are only interested in success result
            value = result.Value;
            /*MODIFY*/

            if (result.ErrorMessage != null)
                argumentResult.AddError(result.ErrorMessage);

            return result.Result == ArgumentConversionResultType.Successful;
        }

        internal static ArgumentConversionResult ConvertObject(
            ArgumentResult argumentResult,
            Type type,
            object? value)
        {
            switch (value)
            {
                case Token singleValue:
                    return ConvertToken(argumentResult, type, singleValue);

                case IReadOnlyList<Token> manyValues:
                    return ConvertTokens(argumentResult, type, manyValues);

                default:

                    if (argumentResult.Tokens.Count == 0)
                    {
                        //Support bool flags here because ConvertIfNeeded cannot detect it as ArgumentConversionResult is internal
                        //and we can not return our implemented ArgumentConversionResult
                        if (type == typeof(bool) || type == typeof(bool?))
                            return Success(argumentResult, true);

                        return None(argumentResult);
                    }
                    else
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        private static ArgumentConversionResult ConvertToken(
            ArgumentResult argumentResult,
            Type type,
            Token token)
        {
            var value = token.Value;

            type = type.GetNullableUnderlyingTypeOrSelf();

            if (StringConverters.TryGetValue(type, out var tryConvert))
            {
                try
                {
                    return tryConvert(value, out var converted)
                        ? Success(argumentResult, converted)
                        : ArgumentConversionCannotParse(argumentResult, type, value);
                }
                catch (Exception exception)
                {
                    return ArgumentConversionException(argumentResult, type, value, exception);
                }
            }

            if (type.IsEnum)
            {
#if NET7_0_OR_GREATER
                if (Enum.TryParse(type, value, ignoreCase: true, out var converted))
                {
                    return Success(argumentResult, converted);
                }
#else
                try
                {
                    return Success(argumentResult, Enum.Parse(type, value, true));
                }
                catch (ArgumentException)
                {
                }
#endif
            }

            return ArgumentConversionCannotParse(argumentResult, type, value);
        }

        private static ArgumentConversionResult ConvertTokens(
            ArgumentResult argumentResult,
            Type type,
            IReadOnlyList<Token> tokens)
        {
            type = type.GetNullableUnderlyingTypeOrSelf();
            var itemType = type.GetElementTypeIfEnumerable(typeof(string)) ?? typeof(string);
            var values = CreateArray(itemType, tokens.Count); //typed array

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                var result = ConvertToken(argumentResult, itemType, token);

                switch (result.Result)
                {
                    case ArgumentConversionResultType.Successful:
                        values.SetValue(result.Value, i);

                        break;
                    /*MODIFY*/
                    //if element conversion fails due to type parsing, collection should also fail
                    //if we don't handle this (return failure), OnlyTake below causes combined errors:
                    // Unrecognized command or argument 'exception'.
                    // Cannot parse argument '' for command 'TestApp' as expected type
                    case ArgumentConversionResultType.FailedType:
                        return result;
                    /*MODIFY*/
                    default: // failures
                        if (argumentResult.Parent is CommandResult)
                        {
                            argumentResult.OnlyTake(i);

                            i = tokens.Count;
                            break;
                        }

                        return result;
                }
            }

            try
            {
                return CollectionConverters.TryGetValue(type, out var convertFromArray)
                    ? Success(argumentResult, convertFromArray(values))
                    : ArgumentConversionCannotParse(argumentResult, type, string.Join("|", tokens));
            }
            catch (Exception exception)
            {
                return ArgumentConversionException(argumentResult, type, string.Join("|", tokens), exception);
            }
        }

        internal static void RegisterCollectionConverter<TCollection>(Func<Array, TCollection>? convertFromArray)
        {
            if (convertFromArray == null)
                return;

            var collectionType = typeof(TCollection).GetNullableUnderlyingTypeOrSelf();

            if (!CollectionConverters.ContainsKey(collectionType))
            {
                object ConvertArray(Array array)
                {
                    //Exceptions are handled in ConvertToken
                    return convertFromArray(array)!;
                }

                CollectionConverters.Add(collectionType, ConvertArray);
            }
        }

        internal static void RegisterStringConverter<TArgument>(Func<string, TArgument>? convertFromString)
        {
            if (convertFromString == null)
                return;

            var itemType = typeof(TArgument).GetNullableUnderlyingTypeOrSelf();

            if (!StringConverters.ContainsKey(itemType))
            {
                bool TryConvertString(string input, out object value)
                {
                    //Exceptions are handled in ConvertTokens
                    value = convertFromString(input)!;
                    return true;
                }

                StringConverters.Add(itemType, TryConvertString);
            }
        }

        internal static object? GetDefaultValue(Type type)
        {
            if (type.IsNullable())
                return null;

            try
            {
                var itemType = type.GetElementTypeIfEnumerable(typeof(string));
                if (itemType != null
                    && CollectionConverters.TryGetValue(type, out var convertFromArray))
                    return convertFromArray(CreateArray(itemType, 0));
            }
            catch
            {
                return null;
            }

            if (type.IsValueType)
                return CreateDefaultValueType(type);

            return null;
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "https://github.com/dotnet/command-line-api/issues/1638")]
        private static Array CreateArray(Type itemType, int capacity)
            => Array.CreateInstance(itemType, capacity);

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2067:UnrecognizedReflectionPattern",
            Justification = $"{nameof(CreateDefaultValueType)} is only called on a ValueType. You can always create an instance of a ValueType.")]
        private static object CreateDefaultValueType(Type type) =>
#if NET
            RuntimeHelpers.GetUninitializedObject(type);
#else
            FormatterServices.GetUninitializedObject(type);
#endif
    }
}
