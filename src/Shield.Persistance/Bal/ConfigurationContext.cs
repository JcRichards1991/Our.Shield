using Newtonsoft.Json;
using System;
using Umbraco.Core;

namespace Shield.Persistance.Bal
{
    public abstract class ConfigurationContext : Record
    {
        protected override T Read<T>()
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            var record = db.SingleOrDefault<Dal.Configuration>(Id);

            if (record == null)
            {
                return new T();
            }
            return JsonConvert.DeserializeObject<T>(record.Value);
        }

        protected override bool Write<T>(T values)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var record = new Dal.Configuration()
            {
                Id = Id,
                LastModified = DateTime.UtcNow,
                Value = JsonConvert.SerializeObject(values)
            };

            db.Save(record);
            return true;
        }

    }
}
