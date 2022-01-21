using System;
using System.Collections.Generic;
using System.Linq;

namespace Our.Shield.Shared.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Tries to find the index of the item in the collection with matching predict
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns>The index of the item if found, otherwise, -1</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var found = false;
            int index = 0;

            foreach (var item in items)
            {
                if (predicate(item))
                {
                    found = true;
                    break;
                }

                index++;
            }

            return found
                ? index
                : -1;
        }

        public static bool HasValues<T>(this IEnumerable<T> items)
        {
            return items?.Any() ?? false;
        }

        public static bool HasValues<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return items?.Any(predicate) ?? false;
        }

        public static bool None<T>(this IEnumerable<T> items)
        {
            return !items?.Any() ?? true;
        }

        public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return !items?.Any(predicate) ?? true;
        }
    }
}
