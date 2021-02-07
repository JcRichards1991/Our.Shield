using Our.Shield.Core.Models;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Services;
using System;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    /// <inheritdoc />
    public class ConfigurationCacheRefresher : JsonCacheRefresherBase<ConfigurationCacheRefresher>
    {
        /// <inheritdoc />
        public override Guid RefresherUniqueId => new Guid(Constants.DistributedCache.ConfigurationCacheRefresherId);

        /// <inheritdoc />
        public override string Name => "Shield Configuration Cache Refresher";

        /// <inheritdoc />
        protected override ConfigurationCacheRefresher This => this;

        /// <inheritdoc />
        public ConfigurationCacheRefresher(AppCaches appCaches) : base (appCaches)
        {
        }

        /// <inheritdoc />
        public override void Refresh(string json)
        {
            //var cacheInstruction = Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigurationCacheRefresherJsonModel>(json);

            //var job = JobService.Instance.Job(cacheInstruction.Key);

            //if (job == null)
            //{
            //    //  Invalid id
            //    return;
            //}

            //switch (cacheInstruction.CacheRefreshType)
            //{
            //    case Enums.CacheRefreshType.Write:
            //        JobService.Instance.Execute((Job)job);
            //        break;

            //    default:
            //        return;
            //}

            base.Refresh(json);
        }
    }
}
