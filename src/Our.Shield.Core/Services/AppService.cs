using Newtonsoft.Json;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.ContractResolvers;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Factories;
using Our.Shield.Core.Models;
using Our.Shield.Core.Models.AppTabs;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Models.Requests;
using Our.Shield.Core.Models.Responses;
using Our.Shield.Shared;
using Our.Shield.Shared.Enums;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements <see cref="IAppService"/>
    /// </summary>
    public class AppService : IAppService
    {
        private readonly IJobService _jobService;
        private readonly IAppAccessor _appAccessor;
        private readonly IAppFactory _appFactory;
        private readonly DistributedCache _distributedCache;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="AppService"/>
        /// </summary>
        /// <param name="jobService"><see cref="IJobService"/></param>
        /// <param name="appAccessor"><see cref="IAppAccessor"/></param>
        /// <param name="appFactory"><see cref="IAppFactory"/></param>
        /// <param name="distributedCache"><see cref="DistributedCache"/></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        public AppService(
            IJobService jobService,
            IAppAccessor appAccessor,
            IAppFactory appFactory,
            DistributedCache distributedCache,
            ILogger logger)
        {
            GuardClauses.NotNull(jobService, nameof(jobService));
            GuardClauses.NotNull(appAccessor, nameof(appAccessor));
            GuardClauses.NotNull(appFactory, nameof(appFactory));
            GuardClauses.NotNull(distributedCache, nameof(distributedCache));
            GuardClauses.NotNull(logger, nameof(logger));

            _jobService = jobService;
            _appAccessor = appAccessor;
            _appFactory = appFactory;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<GetAppResponse> GetApp(Guid key)
        {
            var response = new GetAppResponse();
            var dbApp = await _appAccessor.Read(key);

            if (dbApp == null)
            {
                response.ErrorCode = ErrorCode.AppRead;

                return response;
            }

            var app = _appFactory.Create(dbApp.AppId);
            app.Key = dbApp.Key;

            if (app == null)
            {
                response.ErrorCode = ErrorCode.AppCreate;
                response.Warnings.Add($"No install App Plug-in to handle app configuration with App Id: {dbApp.AppId}");

                return response;
            }

            response.App = app;
            response.Configuration = DeserializeAppConfiguration(dbApp, app.GetType().BaseType?.GenericTypeArguments[0]);
            response.Tabs = GetAppTabs(app);

            return response;
        }

        /// <inheritdoc />
        public async Task<GetAppsResponse> GetApps(Guid environmentKey)
        {
            var response = new GetAppsResponse();
            var dbApps = await _appAccessor.ReadByEnvironmentKey(environmentKey);

            if (dbApps.None())
            {
                response.ErrorCode = ErrorCode.AppsRead;
            }

            var apps = new Dictionary<IApp, IAppConfiguration>();

            foreach (var dbApp in dbApps)
            {
                var app = _appFactory.Create(dbApp.AppId);

                if (app == null)
                {
                    response.Warnings.Add($"No install App Plug-in to handle app configuration with App Id: {dbApp.AppId}");

                    continue;
                }

                app.Key = dbApp.Key;

                apps.Add(app, DeserializeAppConfiguration(dbApp, app.GetType().BaseType?.GenericTypeArguments[0]));
            }

            response.Apps = apps;

            return response;
        }

        /// <inheritdoc />
        public async Task<UpdateAppConfigurationResponse> UpdateAppConfiguration(UpdateAppConfigurationRequest request)
        {
            var app = _appFactory.Create(request.AppId);
            var response = new UpdateAppConfigurationResponse();

            if (app == null)
            {
                response.ErrorCode = ErrorCode.AppCreate;

                return response;
            }

            app.Key = request.Key;

            IAppConfiguration configuration;

            try
            {
                configuration = JsonConvert.DeserializeObject(
                    request.Configuration.ToString(),
                    app.GetType().BaseType?.GenericTypeArguments[0]) as IAppConfiguration;

                if (!await _appAccessor.Update(app.Id, app.Key, configuration))
                {
                    response.ErrorCode = ErrorCode.AppUpdate;

                    return response;
                }
            }
            catch (JsonException ex)
            {
                _logger.Error<AppService>(ex, "Error occurred deserializing App's configuration for Updating");

                response.ErrorCode = ErrorCode.AppDeserializeConfiguration;

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error<AppService>(ex, "Error occurred updating App's configuration. AppId: {AppId}; Key: {AppKey}", app.Key, app.Id);

                response.ErrorCode = ErrorCode.AppUpdate;

                return response;
            }

            _jobService.ExecuteApp(app.Key, configuration);

            _distributedCache.RefreshByJson(
                Guid.Parse(Constants.DistributedCache.ConfigurationCacheRefresherId),
                JsonConvert.SerializeObject(new ConfigurationCacheRefresherJsonModel(Enums.CacheRefreshType.Upsert, app.Key)));

            return response;
        }

        /// <inheritdoc />
        public void WriteEnvironmentApps(Guid enviromentKey, IEnumerable<string> appIds = null)
        {
            appIds = appIds ?? _appFactory.GetRegistedAppsIds();
            var apps = new List<Data.Dtos.App>();

            foreach (var appId in appIds)
            {
                var app = _appFactory.Create(appId);

                apps.Add(new Data.Dtos.App
                {
                    Key = Guid.NewGuid(),
                    LastModifiedDateUtc = DateTime.UtcNow,
                    AppId = appId,
                    Configuration = JsonConvert.SerializeObject(
                        app.DefaultConfiguration,
                        Formatting.None,
                        new JsonSerializerSettings
                        {
                            ContractResolver = new AppConfigurationShouldSerializeContractResolver()
                        }),
                    Enabled = app.DefaultConfiguration.Enabled,
                    EnvironmentKey = enviromentKey
                });
            }

            _appAccessor.Write(apps);
        }

        private IAppConfiguration DeserializeAppConfiguration(Data.Dtos.IApp app, Type appConfigurationType)
        {
            var configuration = JsonConvert.DeserializeObject(app.Configuration, appConfigurationType) as IAppConfiguration;

            configuration.Enabled = app.Enabled;
            configuration.LastModifiedDateUtc = app.LastModifiedDateUtc;

            return configuration;
        }

        private IEnumerable<ITab> GetAppTabs(IApp app)
        {
            var tabAttrs = app.GetType().GetCustomAttributes(typeof(AppTabAttribute), true) as AppTabAttribute[];

            foreach (var tabAttr in tabAttrs.OrderBy(x => x.SortOrder))
            {
                if (tabAttr is AppEditorAttribute appEditorAttr)
                {
                    yield return new AppConfigurationTab(appEditorAttr);
                    continue;
                }

                yield return new Tab(tabAttr);
            }
        }
    }
}
