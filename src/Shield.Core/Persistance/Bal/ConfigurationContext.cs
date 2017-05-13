namespace Shield.Core.Persistance.Bal
{
    using Newtonsoft.Json;
    using System;
    using Umbraco.Core;

    /// <summary>
    /// The Configuration Context.
    /// </summary>
    public static class ConfigurationContext
    {
        /// <summary>
        /// Reads a Configuration from the database.
        /// </summary>
        /// <param name="id">
        /// The id of the configuration.
        /// </param>
        /// <param name="type">
        /// The type of configuration to return;
        /// </param>
        /// <returns>
        /// The Configuration as the desired type.
        /// </returns>
        public static Models.Configuration Read(string id, Type type, Models.Configuration defaultConfiguration)
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var record = db.SingleOrDefault<Dal.Configuration>((object)id);

            if (record == null || string.IsNullOrEmpty(record.Value))
            {
                return defaultConfiguration;
            }

            try
            {
                return JsonConvert.DeserializeObject(record.Value, type) as Models.Configuration;
            }
            catch (JsonSerializationException)
            {
                return defaultConfiguration;
            }
        }

        /// <summary>
        /// Writes a Configuration to the database.
        /// </summary>
        /// <param name="id">
        /// The id of Configuration to write.
        /// </param>
        /// <param name="config">
        /// The configuration to write to the database
        /// </param>
        /// <returns>
        /// If successfull, returns true, otherwise false.
        /// </returns>
        public static bool Write(string id, Models.Configuration config)
        {
            config.LastModified = DateTime.UtcNow;

            var db = ApplicationContext.Current.DatabaseContext.Database;
            var record = new Dal.Configuration
            {
                Id = id,
                Value = JsonConvert.SerializeObject(config)
            };

            if (db.Exists<Dal.Configuration>(id))
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
