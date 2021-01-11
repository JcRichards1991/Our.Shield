using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements <see cref="IJournalService"/>
    /// </summary>
    internal class JournalService : IJournalService
    {
        /// <inheritdoc />
        public IEnumerable<IJournal> JournalListing(int id, int page, int itemsPerPage, Type type, out int totalPages) =>
            throw new NotImplementedException(); //DbContext.Instance.Journal.Read(id, page, itemsPerPage, type, out totalPages);

        /// <inheritdoc />
        public IEnumerable<T> JournalListing<T>(int id, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            throw new NotImplementedException(); //DbContext.Instance.Journal.Read(id, page, itemsPerPage, typeof(T), out totalPages).Select(x => (T)x);
    }
}
