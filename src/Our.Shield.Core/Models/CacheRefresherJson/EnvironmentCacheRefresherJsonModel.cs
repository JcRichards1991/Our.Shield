using System;

namespace Our.Shield.Core.Models.CacheRefresherJson
{
    internal class EnvironmentCacheRefresherJsonModel : ICacheRefreshJsonModel
    {
        public EnvironmentCacheRefresherJsonModel(Enums.CacheRefreshType cacheRefreshType, Guid key)
        {
            CacheRefreshType = cacheRefreshType;
            Key = key;
        }

        public Enums.CacheRefreshType CacheRefreshType { get; set; }

        public Guid Key { get; set; }
    }
}
