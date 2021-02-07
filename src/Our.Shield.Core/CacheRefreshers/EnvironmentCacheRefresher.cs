using AutoMapper;
using LightInject;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Services;
using Our.Shield.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    /// <inheritdoc />
    public class EnvironmentCacheRefresher : JsonCacheRefresherBase<EnvironmentCacheRefresher>
    {
        private readonly IJobService _jobService;
        private readonly IEnvironmentAccessor _dataAccessor;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentCacheRefresher"/>
        /// </summary>
        /// <param name="appCaches"><see cref="AppCaches"/></param>
        /// <param name="jobService"><see cref="IJobService"/></param>
        /// <param name="dataAccessor"><see cref="IEnvironmentAccessor"/></param>
        public EnvironmentCacheRefresher (
            AppCaches appCaches,
            IJobService jobService,
            IEnvironmentAccessor dataAccessor,
            [Inject(nameof(Shield))] IMapper mapper)
            : base(appCaches)
        {
            GuardClauses.NotNull(jobService, nameof(jobService));
            GuardClauses.NotNull(dataAccessor, nameof(dataAccessor));
            GuardClauses.NotNull(mapper, nameof(mapper));

            _jobService = jobService;
            _dataAccessor = dataAccessor;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public override Guid RefresherUniqueId => new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId);

        /// <inheritdoc />
        public override string Name => "Shield Environment Cache Refresher";

        /// <inheritdoc />
        protected override EnvironmentCacheRefresher This => this;

        /// <inheritdoc />
        public override void Refresh(string json)
        {
            var cacheInstruction = Newtonsoft.Json.JsonConvert.DeserializeObject<EnvironmentCacheRefresherJsonModel>(json);
            var environments = _jobService.Environments.Keys;
            var environment = environments.FirstOrDefault(x => x.Key == cacheInstruction.Key);

            switch (cacheInstruction.CacheRefreshType)
            {
                case Enums.CacheRefreshType.Upsert:
                    {
                        var dto = Task
                            .Run(async () => await _dataAccessor.Read(cacheInstruction.Key))
                            .GetAwaiter()
                            .GetResult();

                        var env = _mapper.Map<Models.Environment>(dto);

                        if (environment == null)
                        {
                            _jobService.Register(env);
                        }
                        else
                        {
                            _jobService.Unregister(environment);
                            _jobService.Register(env);
                        }

                        break;
                    }

                case Enums.CacheRefreshType.Remove:
                    {
                        if (environment == null)
                        {
                            return;
                        }
                        else
                        {
                            _jobService.Unregister(environment);
                            break;
                        }
                    }

                case Enums.CacheRefreshType.ReOrder:
                    {
                        var dtos = Task
                            .Run(async () => await _dataAccessor.Read())
                            .GetAwaiter()
                            .GetResult();

                        var envs = _mapper.Map<IList<Models.Environment>>(dtos);

                        foreach (var env in envs)
                        {
                            if (!environments.Any(x => x.Key == env.Key && x.SortOrder != env.SortOrder))
                            {
                                continue;
                            }

                            _jobService.Unregister(env);
                            _jobService.Register(env);
                        }

                        break;
                    }
            }

            base.Refresh(json);
        }
    }
}
