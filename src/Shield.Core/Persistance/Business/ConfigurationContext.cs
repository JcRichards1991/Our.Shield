namespace Shield.Core.Persistance.Business
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
        /// <param name="environmentId">
        /// The environmentId of the configuration.
        /// </param>
        /// <param name="appId">
        /// The appId of the configuration.
        /// </param>
        /// <param name="type">
        /// The type of configuration to return;
        /// </param>
        /// <returns>
        /// The Configuration as the desired type.
        /// </returns>
        public static Serialization.Configuration Read(int environmentId, string appId, Type type, 
            Serialization.Configuration defaultConfiguration)
        {
            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                var record = db.SingleOrDefault<Data.Dto.Configuration>(
                    "WHERE " + 
                    nameof(Data.Dto.Configuration.EnvironmentId) + " = @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);
                if (record == null || string.IsNullOrEmpty(record.Value))
                {
                    return defaultConfiguration;
                }

                return JsonConvert.DeserializeObject(record.Value, type) as Serialization.Configuration;
            }
            catch (JsonSerializationException jEx)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error Deserialising configuration with environmentId: {environmentId} for appId: {appId}; to type:{type}", jEx);
                return defaultConfiguration;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error reading configuration with environmentId: {environmentId} for appId: {appId}; to type:{type}", ex);
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
        public static bool Write(int environmentId, string appId, Serialization.Configuration config)
        {
            config.LastModified = DateTime.UtcNow;

            try
            {
                var db = ApplicationContext.Current.DatabaseContext.Database;
                var record = db.SingleOrDefault<Data.Dto.Configuration>(
                    "WHERE " + 
                    nameof(Data.Dto.Configuration.EnvironmentId) + " = @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);
                if (record == null)
                {
                    record = new Data.Dto.Configuration
                    {
                        EnvironmentId = environmentId,
                        AppId = appId,
                        Value = JsonConvert.SerializeObject(config)
                    };                                 
                    db.Insert(record);
                }
                else
                {
                    record.Value = JsonConvert.SerializeObject(config);
                    db.Update(record);
                }
                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error writing configuration with environmentId: {environmentId} for appId: {appId}", ex);
            }
            return false;
        }
    }
}
