using System;
using System.Collections.Generic;
using System.Linq;

namespace Our.Shield.Shared
{
    public static class GuardClauses
    {
        public static void NotNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName, "Cannot be null");
            }
        }

        public static void NotNullOrWhiteSpace(string str, string paramName)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentNullException(paramName, "Cannot be null, empty or whitespace");
            }
        }

        public static void NotEmpty<T>(IEnumerable<T> list, string paramName)
        {
            if (list?.Any() != true)
            {
                throw new ArgumentNullException(paramName, "Cannot be null or an empty list");
            }
        }
    }
}
