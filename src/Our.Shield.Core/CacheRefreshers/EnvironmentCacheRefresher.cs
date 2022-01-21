using AutoMapper;
using LightInject;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Services;
using Our.Shield.Shared;
using System;
using System.Linq;
using Umbraco.Core.Cache;

namespace Our.Shield.Core.CacheRefreshers
{
    /// <inheritdoc />
    public class EnvironmentCacheRefresher : JsonCacheRefresherBase<EnvironmentCacheRefresher>
    {
        private readonly IJobService _jobService;
        private readonly IEnvironmentService _environmentService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentCacheRefresher"/>
        /// </summary>
        /// <param name="appCaches"><see cref="AppCaches"/></param>
        /// <param name="jobService"><see cref="IJobService"/></param>
        /// <param name="environmentService"><see cref="IEnvironmentService"/></param>
        /// <param name="mapper"><see cref="IMapper"/></param>
        public EnvironmentCacheRefresher (
            AppCaches appCaches,
            IJobService jobService,
            IEnvironmentService environmentService,
            [Inject(nameof(Shield))] IMapper mapper)
            : base(appCaches)
        {
            GuardClauses.NotNull(jobService, nameof(jobService));
            GuardClauses.NotNull(environmentService, nameof(environmentService));
            GuardClauses.NotNull(mapper, nameof(mapper));

            _jobService = jobService;
            _environmentService = environmentService;
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
                        var response = _environmentService.Get(cacheInstruction.Key).Result;

                        if (environment == null)
                        {
                            _jobService.Register(response.Environment);
                        }
                        else
                        {
                            _jobService.Unregister(environment);
                            _jobService.Register(response.Environment);
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
                        throw new NotImplementedException();

                        //var dtos = Task
                        //    .Run(async () => await _dataAccessor.Read())
                        //    .GetAwaiter()
                        //    .GetResult();

                        //var envs = _mapper.Map<IList<Models.Environment>>(dtos);

                        //foreach (var env in envs)
                        //{
                        //    if (!environments.Any(x => x.Key == env.Key && x.SortOrder != env.SortOrder))
                        //    {
                        //        continue;
                        //    }

                        //    _jobService.Unregister(env);
                        //    _jobService.Register(env);
                        //}

                        break;
                    }
            }

            base.Refresh(json);
        }
    }
}
