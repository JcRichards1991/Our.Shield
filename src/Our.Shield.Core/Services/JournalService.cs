using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Models;
using Our.Shield.Shared;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements <see cref="IJournalService"/>
    /// </summary>
    internal class JournalService : IJournalService
    {
        private readonly IJournalAccessor _journalAccessor;

        public JournalService(IJournalAccessor journalAcccessor)
        {
            GuardClauses.NotNull(journalAcccessor, nameof(journalAcccessor));

            _journalAccessor = journalAcccessor;
        }

        /// <inheritdoc />
        public IEnumerable<IJournal> JournalListing(int id, int page, int itemsPerPage, Type type, out int totalPages) =>
            throw new NotImplementedException(); //DbContext.Instance.Journal.Read(id, page, itemsPerPage, type, out totalPages);

        /// <inheritdoc />
        public IEnumerable<T> JournalListing<T>(int id, int page, int itemsPerPage, out int totalPages) where T : IJournal =>
            throw new NotImplementedException(); //DbContext.Instance.Journal.Read(id, page, itemsPerPage, typeof(T), out totalPages).Select(x => (T)x);

        public bool WriteJournal(IJournal journal) =>
            throw new NotImplementedException(); // JobService.Instance.WriteJournal(this, journal);
    }
}
