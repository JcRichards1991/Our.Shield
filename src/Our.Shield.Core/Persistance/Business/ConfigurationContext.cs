using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Our.Shield.Core.Persistance.Business
{
    /// <summary>
    /// The Configuration Context.
    /// </summary>
    public class ConfigurationContext : DbContext
    {

        /// <summary>
        /// 
        /// </summary>
        internal class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();
 
            /// <summary>
            /// 
            /// </summary>
            /// <param name="member"></param>
            /// <param name="memberSerialization"></param>
            /// <returns></returns>
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
 
                if (property.PropertyName.Equals(nameof(IConfiguration.Enable), StringComparison.InvariantCultureIgnoreCase) || 
                    property.PropertyName.Equals(nameof(IConfiguration.LastModified), StringComparison.InvariantCultureIgnoreCase))
                {
                    property.ShouldSerialize = instance =>
                    {
                        return false;
                    };
                }
                return property;
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="defaultConfiguration"></param>
        /// <returns></returns>
        public T Read<T>(int environmentId, string appId, T defaultConfiguration) where T : IConfiguration
        {
            return (T)Read(environmentId, appId, typeof(T), defaultConfiguration);
        }

        private void SetSingleEnvironmentValues(IConfiguration configuration, int environmentId, string appId)
        {
            var configurationType = configuration.GetType();
            var properties = configurationType.GetProperties().Where(x => x.GetCustomAttribute<Attributes.SingleEnvironmentAttribute>() != null);

            if (properties.Any())
            {
                var records = Database.Fetch<Data.Dto.Configuration>(
                    "WHERE " + nameof(Data.Dto.Configuration.EnvironmentId) + " != @0 AND " +
                    nameof(Data.Dto.Configuration.AppId) + " = @1",
                    environmentId, appId);

                foreach (var record in records)
                {
                    var config = JsonConvert.DeserializeObject(record.Value, configurationType) as Configuration;

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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool Write(int environmentId, string appId, IConfiguration config)
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
    }
}
