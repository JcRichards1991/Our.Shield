using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Persistence.Data.Dto;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Our.Shield.Core.Persistence.Business
{
    /// <inheritdoc />
    /// <summary>
    /// The Configuration Context.
    /// </summary>
    public class ConfigurationContext : DbContext
    {
        private class ShouldSerializeContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyName.Equals(nameof(IAppConfiguration.Enable), StringComparison.InvariantCultureIgnoreCase)
                    || property.PropertyName.Equals(nameof(IAppConfiguration.LastModified), StringComparison.InvariantCultureIgnoreCase))
                {
                    property.ShouldSerialize = instance => false;
                }
                return property;
            }
        }

        private void SetDefaultConfigurationSingleEnvironmentValues(string appId, IAppConfiguration configuration)
        {
            var configurationType = configuration.GetType();
            var properties = configurationType.GetProperties().Where(x => x.GetCustomAttribute<SingleEnvironmentAttribute>() != null).ToArray();

            if (!properties.Any())
                return;

            var records = Database.Fetch<Configuration>(
                "WHERE " + nameof(Data.Dto.Configuration.AppId) + " = @0",
                appId);

            var record = records.FirstOrDefault();

            if (record == null)
                return;

            var recordConfiguration = JsonConvert.DeserializeObject(record.Value, configurationType) as AppConfiguration;

            foreach (var property in properties)
            {
                property.SetValue(configuration, property.GetValue(recordConfiguration));
            }
        }
        
        public IAppConfiguration Read(int environmentId, string appId, Type type, IAppConfiguration defaultConfiguration)
        {
            if (!(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(defaultConfiguration), type) is AppConfiguration defaultConfig))
                return null;

            SetDefaultConfigurationSingleEnvironmentValues(appId, defaultConfig);

            defaultConfig.LastModified = null;
            defaultConfig.Enable = false;

            try
            {
                var record = Database.SingleOrDefault<Configuration>(
                    "WHERE " + nameof(Data.Dto.Configuration.EnvironmentId) + " = @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);

                if (string.IsNullOrEmpty(record?.Value))
                    return defaultConfig;

                if (!(JsonConvert.DeserializeObject(record.Value, type) is AppConfiguration config))
                    return defaultConfig;

                config.LastModified = record.LastModified;
                config.Enable = record.Enable;

                return config;
            }
            catch (JsonSerializationException jEx)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error Deserializing configuration with environmentId: {environmentId} for appId: {appId}; to type:{type}", jEx);
                return defaultConfiguration;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error reading configuration with environmentId: {environmentId} for appId: {appId}; to type:{type}", ex);
                return defaultConfiguration;
            }
        }

        public Guid ReadUniqueKey(int envId, string appId)
        {
            try
            {
                var record = Database.SingleOrDefault<Configuration>(new Sql()
                    .Where<Configuration>(x => x.EnvironmentId == envId)
                    .Where<Configuration>(x => x.AppId == appId));

                return record?.Key ?? Guid.NewGuid();
            }
            catch (SqlException sqlEx)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error Reading App's Unique Key. App Name: {appId}; Environment Id: {envId}", sqlEx);
                throw;
            }
        }
        
        public T Read<T>(int environmentId, string appId, T defaultConfiguration) where T : IAppConfiguration
        {
            return (T)Read(environmentId, appId, typeof(T), defaultConfiguration);
        }

        private void SetSingleEnvironmentValues(IAppConfiguration configuration, int environmentId, string appId)
        {
            var configurationType = configuration.GetType();
            var properties = configurationType.GetProperties();

            if (properties.All(x => x.GetCustomAttribute<SingleEnvironmentAttribute>() == null))
                return;
            
            var singleEnvProperties = properties
                .Where(x => x.GetCustomAttribute<SingleEnvironmentAttribute>() != null)
                .ToArray();

            var records = Database.Fetch<Configuration>(
                "WHERE " + nameof(Data.Dto.Configuration.EnvironmentId) + " != @0 AND " +
                nameof(Data.Dto.Configuration.AppId) + " = @1",
                environmentId, appId);

            foreach (var record in records)
            {

                if (!(JsonConvert.DeserializeObject(record.Value, configurationType) is AppConfiguration recordConfiguration))
                    continue;

                foreach (var property in singleEnvProperties)
                {
                    property.SetValue(recordConfiguration, property.GetValue(configuration));

                    record.Value = JsonConvert.SerializeObject(recordConfiguration, Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });
                }

                recordConfiguration.LastModified = DateTime.UtcNow;
                Database.Update(record);
            }
            
        }
        
        public bool Write(int environmentId, string appId, IAppConfiguration config)
        {
            try
            {
                SetSingleEnvironmentValues(config, environmentId, appId);

                var record = Database.SingleOrDefault<Configuration>(
                    "WHERE " +
                    nameof(Data.Dto.Configuration.EnvironmentId) + " = @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);

                var value = JsonConvert.SerializeObject(config, Formatting.None,
                    new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });

                if (record == null)
                {
                    record = new Configuration
                    {
                        EnvironmentId = environmentId,
                        AppId = appId,
                        Enable = config.Enable,
                        LastModified = DateTime.UtcNow,
                        Value = value
                    };
                    Database.Insert(record);
                }
                else
                {
                    record.Enable = config.Enable;
                    record.LastModified = DateTime.UtcNow;
                    record.Value = value;
                    Database.Update(record);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error writing configuration with environmentId: {environmentId} for appId: {appId}", ex);
            }
            return false;
        }
        
        public bool Delete(int environmentId)
        {
            try
            {
                var sql = new Sql();

                sql.Where(nameof(Data.Dto.Configuration.EnvironmentId) + " = @0", environmentId);
                Database.Delete<Configuration>(sql);

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error deleting configurations for environment with Id: {environmentId}", ex);
            }
            return false;
        }

        public void ConfigMapper(string appId, dynamic definition, Func<dynamic, dynamic> map)
        {
            var sql = new Sql();
            sql.Where<Configuration>(x => x.AppId == appId);

            var configs = Database.Fetch<Configuration>(sql);

            foreach (var config in configs)
            {
                //  Deserialize the current config to an anonymous object
                var oldData = JsonConvert.DeserializeAnonymousType(config.Value, definition);

                //  serialize the new configuration to the db entry's value
                config.Value = JsonConvert.SerializeObject(map(oldData), Formatting.None);

                //  Update the entry within the DB.
                Database.Update(config);
            }
        }
    }
}
