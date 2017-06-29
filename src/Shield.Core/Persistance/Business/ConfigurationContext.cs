namespace Shield.Core.Persistance.Business
{
    using Newtonsoft.Json;
    using System;
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Models;

    /// <summary>
    /// The Configuration Context.
    /// </summary>
    internal class ConfigurationContext : DbContext
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
        public IConfiguration Read(int environmentId, string appId, Type type, IConfiguration defaultConfiguration)
        {
            var config = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(defaultConfiguration), type) as Configuration;
            config.LastModified = null;
            config.Enable = false;
            try
            {
                var record = Database.SingleOrDefault<Data.Dto.Configuration>(
                    "WHERE " + nameof(Data.Dto.Configuration.EnvironmentId) + " = @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);
                if (record != null && !string.IsNullOrEmpty(record.Value))
                {
                    config = JsonConvert.DeserializeObject(record.Value, type) as Configuration;
                    config.LastModified = record.LastModified;
                    config.Enable = record.Enable;
                }
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

            return config;

        }

        public T Read<T>(int environmentId, string appId, T defaultConfiguration) where T : IConfiguration
        {
            return (T) Read(environmentId, appId, typeof(T), defaultConfiguration);
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
        public bool Write(int environmentId, string appId, IConfiguration config)
        {
            try
            {
                var record = Database.SingleOrDefault<Data.Dto.Configuration>(
                    "WHERE " + 
                    nameof(Data.Dto.Configuration.EnvironmentId) + " = @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);
                if (record == null)
                {
                    record = new Data.Dto.Configuration
                    {
                        Enable = config.Enable,
                        LastModified = DateTime.UtcNow,
                        Value = JsonConvert.SerializeObject(config)
                    };                                 
                    Database.Insert(record);
                }
                else
                {
                    record.Enable = config.Enable;
                    record.LastModified = DateTime.UtcNow;
                    record.Value = JsonConvert.SerializeObject(config);
                    Database.Update(record);
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
