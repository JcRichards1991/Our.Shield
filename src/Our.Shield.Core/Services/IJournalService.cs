using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IJournalService
    {
        /// <summary>
        /// Was in EnvironmentService :: TODO: write proper summary &amp; refactor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="type"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        IEnumerable<IJournal> JournalListing(int id, int page, int itemsPerPage, Type type, out int totalPages);

        /// <summary>
        /// Was in EnvironmentService :: TODO: write proper summary &amp; refactor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="totalPages"></param>
        /// <returns></returns>
        IEnumerable<T> JournalListing<T>(int id, int page, int itemsPerPage, out int totalPages) where T : IJournal;
    }
}
