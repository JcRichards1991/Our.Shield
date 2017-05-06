using Newtonsoft.Json;
using System;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Shield.Core.Persistance.Bal
{
    /// <summary>
    /// The Configuration Context.
    /// </summary>
    public abstract class ConfigurationContext : Record
    {
        /// <summary>
        /// Reads a Configuration from the database.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Configuration to read.
        /// </typeparam>
        /// <returns>
        /// The Configuration as the desired type.
        /// </returns>
        protected override T Read<T>()
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            var sql = new Sql();

            var record = db.SingleOrDefault<Dal.Configuration>(Id);

            if (record == null)
            {
                return new T();
            }
            return JsonConvert.DeserializeObject<T>(record.Value);
        }

        protected T Read<T>(string id) where T : new()
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;

            var record = db.SingleOrDefault<Dal.Configuration>(id as object);

            if (record == null)
            {
                return new T();
            }
            return JsonConvert.DeserializeObject<T>(record.Value);
        }

        /// <summary>
        /// Writes a Configuration to the database.
        /// </summary>
        /// <typeparam name="T">
        /// The type of Configuration to write.
        /// </typeparam>
        /// <param name="model">
        /// The Model containing the Configuration settings to be serialized.
        /// </param>
        /// <returns>
        /// If successfull, returns true, otherwise false.
        /// </returns>
        protected override bool Write<T>(T model)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var record = new Dal.Configuration()
            {
                Id = Id,
                LastModified = DateTime.UtcNow,
                Value = JsonConvert.SerializeObject(model)
            };

            if (db.Exists<Dal.Configuration>(Id))
            {
                db.Update(record);
            }
            else
            {
                db.Insert(record);
            }
            return true;
        }
    }
}
