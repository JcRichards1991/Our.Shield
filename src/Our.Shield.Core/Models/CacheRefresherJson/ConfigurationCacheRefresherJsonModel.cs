using System;

namespace Our.Shield.Core.Models.CacheRefresherJson
{
    public class ConfigurationCacheRefresherJsonModel : ICacheRefreshJsonModel
    {
        public ConfigurationCacheRefresherJsonModel(Enums.CacheRefreshType cacheRefreshType, Guid key)
        {
            CacheRefreshType = cacheRefreshType;
            Key = key;
        }

        public Enums.CacheRefreshType CacheRefreshType { get; set; }

        public Guid Key { get; set; }
    }
}
