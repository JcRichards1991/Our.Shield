using Newtonsoft.Json;
using System;
using Umbraco.Core;

namespace Shield.Persistance.Bal
{
    public abstract class ConfigurationRecord : Record
    {
        public T Read<T>() where T : JsonValues, new()
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            var record = db.SingleOrDefault<Dal.Configuration>(Id);

            if (record == null)
            {
                return new T();
            }
            return JsonConvert.DeserializeObject<T>(record.Value);
        }

        public bool Write<T>(T values) where T : Bal.JsonValues
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
