using System;
using System.Collections.Generic;

namespace PathFinder.Components
{
    internal static class EnumerableExtensions
    {
        internal static void ForAll<T>(this IEnumerable<T> iEnumerable, Action<T> action)
        {
            foreach (var item in iEnumerable)
            {
                action(item);
            }
        }

        internal static IEnumerable<T> Tap<T>(this IEnumerable<T> iEnumerable, Action<T> action)
        {
            foreach (var item in iEnumerable)
            {
                action(item);
                yield return item;
            }
        }
    }
}
