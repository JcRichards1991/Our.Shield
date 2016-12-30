using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Shield.Persistance.Bal
{
    public abstract class JournalRecord : Record
    {
        public IEnumerable<T> Read<T>(int page, int itemsPerPage)
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

        public bool Write<T>(T values) where T : Bal.JsonValues
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
