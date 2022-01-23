using Newtonsoft.Json;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Services;
using System;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    /// <inheritdoc />
    public class ConfigurationCacheRefresher : JsonCacheRefresherBase<ConfigurationCacheRefresher>
    {
        private readonly IJobService _jobService;
        private readonly IAppService _appService;

        /// <inheritdoc />
        public ConfigurationCacheRefresher(
            AppCaches appCaches,
            IJobService jobService,
            IAppService appService) : base(appCaches)
        {
            _jobService = jobService;
            _appService = appService;
        }

        /// <inheritdoc />
        public override Guid RefresherUniqueId => new Guid(Constants.DistributedCache.ConfigurationCacheRefresherId);

        /// <inheritdoc />
        public override string Name => "Shield Configuration Cache Refresher";

        /// <inheritdoc />
        protected override ConfigurationCacheRefresher This => this;

        /// <inheritdoc />
        public override void Refresh(string json)
        {
            var cacheInstruction = JsonConvert.DeserializeObject<ConfigurationCacheRefresherJsonModel>(json);

            switch (cacheInstruction.CacheRefreshType)
            {
                case Enums.CacheRefreshType.Upsert:
                    var result = _appService.GetApp(cacheInstruction.Key).Result;
                    _jobService.ExecuteApp(cacheInstruction.Key, result.Configuration);
                    break;

                default:
                    return;
            }

            base.Refresh(json);
        }
    }
}
