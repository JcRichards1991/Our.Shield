using System;

namespace Our.Shield.Core.Models.CacheRefresherJson
{
    public class ConfigurationCacheRefresherJsonModel : ICacheRefreshJsonModel
    {
        public ConfigurationCacheRefresherJsonModel(Enums.CacheRefreshType cacheRefreshType, Guid appKey)
        {
            CacheRefreshType = cacheRefreshType;
            Key = appKey;
        }

        public Enums.CacheRefreshType CacheRefreshType { get; set; }
        public Guid Key { get; set; }
    }
}
