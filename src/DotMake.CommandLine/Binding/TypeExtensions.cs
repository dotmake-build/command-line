// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DotMake.CommandLine.Binding
{
    internal static class TypeExtensions
    {
        /// <param name="type">The type.</param>
        /// <param name="nonGenericElementType">The element type to use for non-generic IEnumerable interfaces like IList (instead of object).</param>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070", Justification = "We probably pass known and used types")]
        internal static Type? GetElementTypeIfEnumerable(this Type type, Type? nonGenericElementType)
        {
            //not common but just in case if it's wrapped in Nullable<T> (struct IEnumerable<T> ?)
            type = type.GetNullableUnderlyingTypeOrSelf();

            if (type.IsArray)
                return type.GetElementType();

            if (type == typeof(string))
                return null;

            // If type is IEnumerable<T> itself
            if (type.GenericTypeArguments.Length == 1
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GenericTypeArguments[0];

            // If type implements/extends IEnumerable<T>
            var enumerableType = type.GetInterfaces()
                .Where(i => i.GenericTypeArguments.Length == 1
                                    && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(i => i.GenericTypeArguments[0])
                .FirstOrDefault();
            if (enumerableType != null)
                return enumerableType;

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return nonGenericElementType;

            return null;
        }

        internal static bool IsNullable(this Type t) => Nullable.GetUnderlyingType(t) is not null;


        internal static Type GetNullableUnderlyingTypeOrSelf(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
