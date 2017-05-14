namespace Shield.Core.Persistance.Bal
{
    using Newtonsoft.Json;
    using System;
    using Umbraco.Core;
    using Umbraco.Core.Logging;

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
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                var record = db.SingleOrDefault<Dal.Configuration>((object)id);

                if (record == null || string.IsNullOrEmpty(record.Value))
                {
                    return defaultConfiguration;
                }

                return JsonConvert.DeserializeObject(record.Value, type) as Models.Configuration;
            }
            catch (JsonSerializationException jEx)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error Deserialising configuration with id: {id}; to type:{type}", jEx);
                return null;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error reading configuration with id: {id}", ex);
                return null;
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

            var record = new Dal.Configuration
            {
                Id = id,
                Value = JsonConvert.SerializeObject(config)
            };

            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
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
            catch(Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error writing configuration with id: {id}; With data: {record.Value}", ex);
            }
            return false;
        }
    }
}
