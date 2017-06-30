namespace Shield.Core.Persistance.Business
{
    using Newtonsoft.Json;
    using System;
    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Models;
    using Newtonsoft.Json.Serialization;
    using System.Reflection;

    /// <summary>
    /// The Configuration Context.
    /// </summary>
    internal class ConfigurationContext : DbContext
    {

        internal class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();
 
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

        public bool Write(int environmentId, string appId, IConfiguration config)
        {
            try
            {
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
    }
}
