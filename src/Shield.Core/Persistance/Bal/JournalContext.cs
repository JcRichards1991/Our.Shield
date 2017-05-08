namespace Shield.Core.Persistance.Bal
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Core;
    using Umbraco.Core.Persistence;

    /// <summary>
    /// The Journal context
    /// </summary>
    public static class JournalContext
    {
        /// <summary>
        /// Reads a Journal from the database.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Journal to read.
        /// </typeparam>
        /// <param name="page">
        /// The page of results to return
        /// </param>
        /// <param name="itemsPerPage">
        /// The number of items per page
        /// </param>
        /// <returns>
        /// The Journal as the desired type.
        /// </returns>
        public static IEnumerable<Models.Journal> Read(string id, int page, int itemsPerPage, Type type)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            var sql = new Sql()
               .Select("*")
               .From()
               .Where("configuration = @0", id)
               .OrderByDescending("datestamp");

            var records = db.Page<Dal.Journal>(page, itemsPerPage, sql);
            if (records?.Items?.Count == 0)
            {
                return Enumerable.Empty<Models.Journal>();
            }

            return records.Items.Select(x =>
            {
                var retItem = JsonConvert.DeserializeObject(x.Value, type) as Models.Journal;
                retItem.Datestamp = x.Datestamp;
                return retItem;
            });
        }

        /// <summary>
        /// Writes a Journal to the database.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Journal to write.
        /// </typeparam>
        /// <param name="values">
        /// Journal object to write.
        /// </param>
        /// <returns>
        /// If successful, returns true; otherwise false.
        /// </returns>
        public static bool Write(string id, Models.Journal journal)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var record = new Dal.Journal()
            {
                ConfigurationId = id,
                Datestamp = DateTime.UtcNow,
                Value = JsonConvert.SerializeObject(journal)
            };

            db.Insert(record);
            return true;
        }

    }
}
