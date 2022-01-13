using Newtonsoft.Json;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Factories;
using Our.Shield.Core.Models;
using Our.Shield.Core.Models.AppTabs;
using Our.Shield.Core.Models.Responses;
using Our.Shield.Shared;
using Our.Shield.Shared.Enums;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements <see cref="IAppService"/>
    /// </summary>
    public class AppService : IAppService
    {
        private readonly IAppAccessor _appAccessor;
        private readonly IAppFactory _appFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="AppService"/>
        /// </summary>
        /// <param name="appAccessor"><see cref="IAppAccessor"/></param>
        /// <param name="appFactory"><see cref="IAppFactory"/></param>
        public AppService(
            IAppAccessor appAccessor,
            IAppFactory appFactory)
        {
            GuardClauses.NotNull(appAccessor, nameof(appAccessor));
            GuardClauses.NotNull(appFactory, nameof(appFactory));

            _appAccessor = appAccessor;
            _appFactory = appFactory;
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

            var app = _appFactory.Create(dbApp.AppId, dbApp.Key);

            if (app == null)
            {
                response.ErrorCode = ErrorCode.AppCreate;
                response.Warnings.Add($"No install App Plug-in to handle app configuration with App Id: {dbApp.AppId}");

                return response;
            }

            response.App = app;
            response.Configuration = DeserializeAppConfiguration(dbApp.Configuration, app.GetType().BaseType?.GenericTypeArguments[0]);
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
                var app = _appFactory.Create(dbApp.AppId, dbApp.Key);

                if (app == null)
                {
                    response.Warnings.Add($"No install App Plug-in to handle app configuration with App Id: {dbApp.AppId}");

                    continue;
                }

                apps.Add(app, DeserializeAppConfiguration(dbApp.Configuration, app.GetType().BaseType?.GenericTypeArguments[0]));
            }

            response.Apps = apps;

            return response;
        }

        private IAppConfiguration DeserializeAppConfiguration(string configurationJson, Type appConfigurationType) =>
            JsonConvert.DeserializeObject(configurationJson, appConfigurationType) as IAppConfiguration;

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
