using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Models;
using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Our.Shield.Core.Persistance.Business
{
    /// <inheritdoc />
    /// <summary>
    /// The Configuration Context.
    /// </summary>
    public class ConfigurationContext : DbContext
    {
        internal class ShouldSerializeContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
 
                if (property.PropertyName.Equals(nameof(IAppConfiguration.Enable), StringComparison.InvariantCultureIgnoreCase) || 
                    property.PropertyName.Equals(nameof(IAppConfiguration.LastModified), StringComparison.InvariantCultureIgnoreCase))
                {
                    property.ShouldSerialize = instance => false;
                }
                return property;
            }
        }

        private void SetDefaultConfigurationSingleEnvironmentValues(string appId, IAppConfiguration configuration)
        {
            var configurationType = configuration.GetType();
            var properties = configurationType.GetProperties().Where(x => x.GetCustomAttribute<Attributes.SingleEnvironmentAttribute>() != null).ToArray();

            if (!properties.Any())
                return;

            var records = Database.Fetch<Data.Dto.Configuration>(
                "WHERE " + nameof(Data.Dto.Configuration.AppId) + " = @0",
                appId);

            if (!records.Any())
                return;

            var record = records.FirstOrDefault();

            if (record == null)
                return;

            var config = JsonConvert.DeserializeObject(record.Value, configurationType) as AppConfiguration;

            foreach (var property in properties)
            {
                property.SetValue(configuration, property.GetValue(config));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="defaultConfiguration"></param>
        /// <returns></returns>
        public IAppConfiguration Read(int environmentId, string appId, Type type, IAppConfiguration defaultConfiguration)
        {
            if (!(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(defaultConfiguration), type) is AppConfiguration defaultConfig))
                return null;

            SetDefaultConfigurationSingleEnvironmentValues(appId, defaultConfig);

            defaultConfig.LastModified = null;
            defaultConfig.Enable = false;

            try
            {
                var record = Database.SingleOrDefault<Data.Dto.Configuration>(
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
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="defaultConfiguration"></param>
        /// <returns></returns>
        public T Read<T>(int environmentId, string appId, T defaultConfiguration) where T : IAppConfiguration
        {
            return (T)Read(environmentId, appId, typeof(T), defaultConfiguration);
        }

        private void SetSingleEnvironmentValues(IAppConfiguration configuration, int environmentId, string appId)
        {
            var configurationType = configuration.GetType();
            var properties = configurationType.GetProperties().Where(x => x.GetCustomAttribute<Attributes.SingleEnvironmentAttribute>() != null).ToArray();

            if (!properties.Any())
                return;

            var records = Database.Fetch<Data.Dto.Configuration>(
                "WHERE " + nameof(Data.Dto.Configuration.EnvironmentId) + " != @0 AND " +
                nameof(Data.Dto.Configuration.AppId) + " = @1",
                environmentId, appId);

            foreach (var record in records)
            {
                var config = JsonConvert.DeserializeObject(record.Value, configurationType) as AppConfiguration;

                if (config == null)
                    continue;

                foreach (var property in properties)
                {
                    property.SetValue(config, property.GetValue(configuration));

                    record.Value = JsonConvert.SerializeObject(config, Formatting.None,
                        new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });
                }

                config.LastModified = DateTime.UtcNow;
                Database.Update(record);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool Write(int environmentId, string appId, IAppConfiguration config)
        {
            try
            {
                SetSingleEnvironmentValues(config, environmentId, appId);

                var record = Database.SingleOrDefault<Data.Dto.Configuration>(
                    "WHERE " + 
                    nameof(Data.Dto.Configuration.EnvironmentId) + " = @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);

                var value = JsonConvert.SerializeObject(config, Formatting.None,
                    new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() });

                if (record == null)
                {
                    record = new Data.Dto.Configuration
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
            catch(Exception ex)
            {
                LogHelper.Error(typeof(ConfigurationContext), $"Error writing configuration with environmentId: {environmentId} for appId: {appId}", ex);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <returns></returns>
        public bool Delete(int environmentId)
        {
            try
            {
                var sql = new Sql();

                sql.Where(nameof(Data.Dto.Configuration.EnvironmentId) + " = @0", environmentId);
                Database.Delete<Data.Dto.Configuration>(sql);

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
            sql.Where<Data.Dto.Configuration>(x => x.AppId == appId);

            var configs = Database.Fetch<Data.Dto.Configuration>(sql);

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
