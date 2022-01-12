using AutoMapper;
using LightInject;
using Newtonsoft.Json;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Factories;
using Our.Shield.Core.Models;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Models.Requests;
using Our.Shield.Core.Models.Responses;
using Our.Shield.Shared;
using Our.Shield.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IEnvironmentService"/> interface
    /// </summary>
    public class EnvironmentService : IEnvironmentService
    {
        private readonly IJobService _jobService;
        private readonly IEnvironmentAccessor _environmentAccessor;
        private readonly IAppAccessor _appAccessor;
        private readonly IAppFactory _appFactory;
        private readonly DistributedCache _distributedCache;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentService"/> class
        /// </summary>
        /// <param name="jobService"><see cref="IJobService"/></param>
        /// <param name="environmentAccessor"><see cref="IEnvironmentAccessor"/></param>
        /// <param name="appAccessor"></param>
        /// <param name="appFactory"><see cref="IAppFactory"/></param>
        /// <param name="distributedCache"><see cref="DistributedCache"/></param>
        /// <param name="mapper"></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        public EnvironmentService(
            IJobService jobService,
            IEnvironmentAccessor environmentAccessor,
            IAppAccessor appAccessor,
            IAppFactory appFactory,
            DistributedCache distributedCache,
            [Inject(nameof(Shield))] IMapper mapper,
            ILogger logger)
        {
            GuardClauses.NotNull(jobService, nameof(jobService));
            GuardClauses.NotNull(environmentAccessor, nameof(environmentAccessor));
            GuardClauses.NotNull(appAccessor, nameof(appAccessor));
            GuardClauses.NotNull(appFactory, nameof(appFactory));
            GuardClauses.NotNull(distributedCache, nameof(distributedCache));
            GuardClauses.NotNull(mapper, nameof(mapper));
            GuardClauses.NotNull(logger, nameof(logger));

            _jobService = jobService;
            _environmentAccessor = environmentAccessor;
            _appAccessor = appAccessor;
            _appFactory = appFactory;
            _distributedCache = distributedCache;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inherit />
        public async Task<UpsertEnvironmentResponse> UpsertAsync(UpsertEnvironmentRequest request)
        {
            var environment = new Models.Environment
            {
                Key = request.Key,
                Name = request.Name,
                Icon = request.Icon,
                Domains = request.Domains,
                Enabled = request.Enabled,
                ContinueProcessing = request.ContinueProcessing,
                SortOrder = request.SortOrder
            };

            var response = await UpsertAsync(environment);

            if (response.HasError())
            {
                return response;
            }

            _distributedCache.RefreshByJson(
                new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
                JsonConvert.SerializeObject(new EnvironmentCacheRefresherJsonModel(CacheRefreshType.Upsert, response.Key)));

            return response;
        }

        /// <inherit />
        public async Task<GetEnvironmentsResponse> GetAsync()
        {
            var response = new GetEnvironmentsResponse();

            try
            {
                var envs = await _environmentAccessor.Read();

                response.Environments = _mapper.Map<List<Models.Environment>>(envs);
            }
            catch (Exception ex)
            {
                _logger.Error<EnvironmentService>(ex, "Error occurred reading environments");

                response.ErrorCode = ErrorCode.EnvironmentRead;
            }

            return response;
        }

        /// <inherit />
        public GetEnvironmentsResponse Get() => GetAsync().Result;

        /// <inherit />
        public async Task<GetEnvironmentResponse> GetAsync(Guid key)
        {
            var response = new GetEnvironmentResponse();

            try
            {
                var environment = await _environmentAccessor.Read(key);

                if (environment != null)
                {
                    response.Environment = _mapper.Map<Models.Environment>(environment);
                }
            }
            catch (Exception ex)
            {
                _logger.Error<EnvironmentService>(ex, "Error occurred reading environment with Key: {Key}", key);

                response.ErrorCode = ErrorCode.EnvironmentRead;
            }

            return response;
        }

        /// <inherit />
        public GetEnvironmentResponse Get(Guid key) => GetAsync(key).Result;

        /// <inherit />
        public async Task<DeleteEnvironmentResponse> DeleteAsync(Guid key)
        {
            GuardClauses.NotNull(key, nameof(key));

            _distributedCache.RefreshByJson(
                new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
                JsonConvert.SerializeObject(new EnvironmentCacheRefresherJsonModel(CacheRefreshType.Remove, key)));

            var response = new DeleteEnvironmentResponse();

            try
            {
                response.Successful = await _environmentAccessor.Delete(key);
            }
            catch(Exception ex)
            {
                _logger.Error<EnvironmentService>(ex, "Error occurred deleting environment with {Key}", key);

                response.ErrorCode = ErrorCode.EnvrionmentDelete;
            }

            return response;
        }

        /// <inherit />
        public async Task<GetEnvironmentAppsResponse> GetAppsForEnvironment(Guid environmentKey)
        {
            var response = new GetEnvironmentAppsResponse();
            var apps = new List<IApp>();

            try
            {
                var result = await _appAccessor.ReadByEnvironmentKey(environmentKey);

                foreach (var dbApp in result)
                {
                    var app = _appFactory.Create(dbApp.AppId, dbApp.Key);

                    apps.Add(app);
                }

                response.Apps = apps;
            }
            catch(Exception ex)
            {
                _logger?.Error<EnvironmentService>(ex, "Error occurred reading apps for environment with Key: {key}", environmentKey);
            }

            return response;
        }

        private async Task<UpsertEnvironmentResponse> UpsertAsync(IEnvironment environment)
        {
            var response = new UpsertEnvironmentResponse();

            if (environment.Key == default(Guid))
            {
                try
                {
                    response.Key = await _environmentAccessor.Create(environment);
                }
                catch (Exception ex)
                {
                    _logger.Error<EnvironmentService>(ex, "Error occurred inserting Environment");

                    response.ErrorCode = ErrorCode.EnviromentInsert;
                }

                return response;
            }

            try
            {
                if (await _environmentAccessor.Update(environment))
                {
                    response.Key = environment.Key;
                }
            }
            catch (Exception ex)
            {
                _logger.Error<EnvironmentService>(ex, "Error occurred updating Environment");

                response.ErrorCode = ErrorCode.EnvironmentUpdate;
            }

            return response;
        }
    }
}
