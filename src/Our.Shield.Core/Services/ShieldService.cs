using Our.Shield.Core.Factories;
using Our.Shield.Shared;
using Our.Shield.Shared.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements <see cref="IShieldService"/>
    /// </summary>
    public class ShieldService : IShieldService
    {
        private readonly IEnvironmentService _environmentService;
        private readonly IAppService _appService;
        private readonly IJobService _jobService;
        private readonly IAppFactory _appFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="ShieldService"/>
        /// </summary>
        /// <param name="environmentService"><see cref="IEnvironmentService"/></param>
        /// <param name="appService"><see cref="IAppService"/></param>
        /// <param name="jobService"><see cref="IJobService"/></param>
        /// <param name="appFactory"><see cref="IAppFactory"/></param>
        public ShieldService(
            IEnvironmentService environmentService,
            IAppService appService,
            IJobService jobService,
            IAppFactory appFactory)
        {
            GuardClauses.NotNull(environmentService, nameof(environmentService));
            GuardClauses.NotNull(appService, nameof(appService));
            GuardClauses.NotNull(jobService, nameof(jobService));
            GuardClauses.NotNull(appFactory, nameof(appFactory));

            _environmentService = environmentService;
            _appService = appService;
            _jobService = jobService;
            _appFactory = appFactory;
        }

        /// <inheritdoc />
        public async Task Init()
        {
            var environmentsResult = await _environmentService.Get();

            foreach (var environment in environmentsResult.Environments)
            {
                var appsResult = await _appService.GetApps(environment.Key);

                foreach (var app in appsResult.Apps)
                {
                    _jobService.Register(environment, app.Key, app.Value);
                }

                var appIds = _appFactory.GetRegistedAppsIds().Where(x => appsResult.Apps.None(y => y.Key.Id == x));

                if (appIds.HasValues())
                {
                    _appService.WriteEnvironmentApps(environment.Key, appIds);
                }
            }
        }
    }
}
