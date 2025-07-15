using System;
using System.Collections;
using System.Collections.Generic;

namespace DotMake.CommandLine.Util
{
    /// <summary>
    /// Provides a set of <see langword="static"/> methods for querying objects that implement <see cref="IEnumerable"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> FlattenBreadthFirst<T>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getChildren)
        {
            var queue = new Queue<T>();

            foreach (var item in source)
            {
                queue.Enqueue(item);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var item in getChildren(current))
                {
                    queue.Enqueue(item);
                }

                yield return current;
            }
        }

        internal static IEnumerable<T> WhileNotNull<T>(
            this T source,
            Func<T, T> next)
            where T : class
        {
            while (source != null)
            {
                yield return source;

                source = next(source);
            }
        }
    }
}
