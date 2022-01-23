using Our.Shield.Core.Enums;
using System;

namespace Our.Shield.Core.Models.CacheRefresherJson
{
    /// <summary>
    /// Implements <see cref="ICacheRefreshJsonModel"/> for cache busting Shields Apps for load balance setups
    /// </summary>
    public class ConfigurationCacheRefresherJsonModel : ICacheRefreshJsonModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cacheRefreshType"></param>
        /// <param name="appKey"></param>
        public ConfigurationCacheRefresherJsonModel(CacheRefreshType cacheRefreshType, Guid appKey)
        {
            CacheRefreshType = cacheRefreshType;
            Key = appKey;
        }

        /// <summary>
        /// The type of cache refresh action
        /// </summary>
        public CacheRefreshType CacheRefreshType { get; set; }

        /// <summary>
        /// The key of the App to cache bust
        /// </summary>
        public Guid Key { get; set; }
    }
}
