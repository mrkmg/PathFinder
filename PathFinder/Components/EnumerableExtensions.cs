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
    }
}
