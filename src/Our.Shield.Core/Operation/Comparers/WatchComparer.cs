using Our.Shield.Core.Operation.Models;
using System.Collections.Generic;

namespace Our.Shield.Core.Operation.Comparers
{
    internal class WatchComparer : IComparer<Watcher>
    {
        public int Compare(Watcher a, Watcher b)
        {
            // ReSharper disable once PossibleNullReferenceException
            return a.Priority - b.Priority;
        }
    }
}
