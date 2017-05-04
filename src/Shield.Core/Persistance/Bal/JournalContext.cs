using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Shield.Core.Persistance.Bal
{
    /// <summary>
    /// The Journal context
    /// </summary>
    public abstract class JournalContext : Record
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
        protected override IEnumerable<T> Read<T>(int page, int itemsPerPage)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            var sql = new Sql()
               .Select("*")
               .From()
               .Where("configuration = @0", Id)
               .OrderByDescending("createdate");

            var records = db.Page<Dal.Journal>(page, itemsPerPage, sql);
            if (records == null || records.Items == null || records.Items.Count == 0)
            {
                return Enumerable.Empty<T>();
            }

            return records.Items.Select(x => JsonConvert.DeserializeObject<T>(x.Value));
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
        protected override bool Write<T>(T values)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var record = new Dal.Journal()
            {
                ConfigurationId = Id,
                CreateDate = DateTime.UtcNow,
                Value = JsonConvert.SerializeObject(values)
            };

            db.Insert(record);
            return true;
        }

    }
}
