using System;
using System.Collections.Generic;

namespace PathFinder.Components
{
    public static class EnumerableExtensions
    {
        public static void ForAll<T>(this IEnumerable<T> iEnumerable, Action<T> action)
        {
            foreach (var item in iEnumerable)
            {
                action(item);
            }
        }

        public static void ForAllWhile<T>(this IEnumerable<T> iEnumerable, Func<T, bool> action) => ForAllWhile(iEnumerable, action, b => b);
        public static void ForAllWhile<T, TT>(this IEnumerable<T> iEnumerable, Func<T, TT> action, Func<TT, bool> check)
        {
            foreach (var item in iEnumerable)
            {
                if (!check(action(item))) return;
            }
        }
    }
}
