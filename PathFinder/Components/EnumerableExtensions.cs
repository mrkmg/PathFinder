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

        internal static void ForAllWhile<T>(this IEnumerable<T> iEnumerable, Func<T, bool> action) => ForAllWhile(iEnumerable, action, b => b);
        internal static void ForAllWhile<T, TT>(this IEnumerable<T> iEnumerable, Func<T, TT> action, Func<TT, bool> check)
        {
            foreach (var item in iEnumerable)
            {
                if (!check(action(item))) return;
            }
        }
    }
}
