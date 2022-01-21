using Our.Shield.Core.Enums;
using System;

namespace Our.Shield.Core.Models.CacheRefresherJson
{
    public class ConfigurationCacheRefresherJsonModel : ICacheRefreshJsonModel
    {
        public ConfigurationCacheRefresherJsonModel(CacheRefreshType cacheRefreshType, Guid appKey)
        {
            CacheRefreshType = cacheRefreshType;
            Key = appKey;
        }

        public CacheRefreshType CacheRefreshType { get; set; }

        public Guid Key { get; set; }
    }
}
