using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Services;
using Our.Shield.Shared;
using System;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    /// <inheritdoc />
    public class EnvironmentCacheRefresher : JsonCacheRefresherBase<EnvironmentCacheRefresher>
    {
        private readonly IJobService _jobService;
        private readonly IEnvironmentService _environmentService;
        private readonly IAppService _appService;

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentCacheRefresher"/>
        /// </summary>
        /// <param name="appCaches"><see cref="AppCaches"/></param>
        /// <param name="jobService"><see cref="IJobService"/></param>
        /// <param name="environmentService"><see cref="IEnvironmentService"/></param>
        /// <param name="appService"><see cref="IAppService"/></param>
        public EnvironmentCacheRefresher(
            AppCaches appCaches,
            IJobService jobService,
            IEnvironmentService environmentService,
            IAppService appService)
            : base(appCaches)
        {
            GuardClauses.NotNull(jobService, nameof(jobService));
            GuardClauses.NotNull(environmentService, nameof(environmentService));
            GuardClauses.NotNull(appService, nameof(appService));

            _jobService = jobService;
            _environmentService = environmentService;
            _appService = appService;
        }

        /// <inheritdoc />
        public override Guid RefresherUniqueId => new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId);

        /// <inheritdoc />
        public override string Name => "Shield Environment Cache Refresher";

        /// <inheritdoc />
        protected override EnvironmentCacheRefresher This => this;

        /// <inheritdoc />
        public override async void Refresh(string json)
        {
            var cacheInstruction = Newtonsoft.Json.JsonConvert.DeserializeObject<EnvironmentCacheRefresherJsonModel>(json);

            switch (cacheInstruction.CacheRefreshType)
            {
                case Enums.CacheRefreshType.Upsert:
                    {
                        var envResult = await _environmentService.Get(cacheInstruction.Key);
                        if (envResult.Environment != null)
                        {
                            _jobService.Unregister(envResult.Environment);
                        }

                        var appsResult = await _appService.GetApps(cacheInstruction.Key);
                        foreach (var app in appsResult.Apps)
                        {
                            _jobService.Register(envResult.Environment, app.Key, app.Value);
                        }

                        break;
                    }

                case Enums.CacheRefreshType.Remove:
                    {
                        var envResult = await _environmentService.Get(cacheInstruction.Key);
                        if (envResult.Environment != null)
                        {
                            _jobService.Unregister(envResult.Environment);
                        }
                        break;
                    }

                case Enums.CacheRefreshType.ReOrder:
                    {
                        var environments = await _environmentService.Get();

                        foreach (var env in environments.Environments)
                        {
                            _jobService.Unregister(env);

                            var appsResult = await _appService.GetApps(cacheInstruction.Key);
                            foreach (var app in appsResult.Apps)
                            {
                                _jobService.Register(env, app.Key, app.Value);
                            }
                        }

                        break;
                    }
            }

            base.Refresh(json);
        }
    }
}
