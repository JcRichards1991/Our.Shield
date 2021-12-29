using System;
using System.Collections.Generic;

namespace Our.Shield.Shared.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Trys to find the index of the item in the collection with matching predict
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns>The index of the item if found, otherwise, -1</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var found = false;
            int index = 0;

            foreach(var item in items)
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
    }
}
