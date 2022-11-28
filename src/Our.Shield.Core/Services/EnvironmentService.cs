using AutoMapper;
using LightInject;
using Newtonsoft.Json;
using Our.Shield.Core.Data.Accessors;
using Our.Shield.Core.Enums;
using Our.Shield.Core.Models;
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
using Umbraco.Core.Services;
using Umbraco.Web;
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
        private readonly IAppService _appService;
        private readonly IUmbracoContextAccessor _umbContextAccessor;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly DistributedCache _distributedCache;

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentService"/> class
        /// </summary>
        /// <param name="jobService"><see cref="IJobService"/></param>
        /// <param name="environmentAccessor"><see cref="IEnvironmentAccessor"/></param>
        /// <param name="appService"><see cref="IAppService"/></param>
        /// <param name="umbContextAccessor"><see cref="IUmbracoContextAccessor"/></param>
        /// <param name="localizedTextService"><see cref="ILocalizedTextService"/></param>
        /// <param name="mapper"></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="distributedCache"><see cref="DistributedCache"/></param>
        public EnvironmentService(
            IJobService jobService,
            IEnvironmentAccessor environmentAccessor,
            IAppService appService,
            IUmbracoContextAccessor umbContextAccessor,
            ILocalizedTextService localizedTextService,
            [Inject(nameof(Shield))] IMapper mapper,
            ILogger logger,
            DistributedCache distributedCache)
        {
            GuardClauses.NotNull(jobService, nameof(jobService));
            GuardClauses.NotNull(environmentAccessor, nameof(environmentAccessor));
            GuardClauses.NotNull(appService, nameof(appService));
            GuardClauses.NotNull(umbContextAccessor, nameof(umbContextAccessor));
            GuardClauses.NotNull(localizedTextService, nameof(localizedTextService));
            GuardClauses.NotNull(mapper, nameof(mapper));
            GuardClauses.NotNull(logger, nameof(logger));
            GuardClauses.NotNull(distributedCache, nameof(distributedCache));

            _jobService = jobService;
            _environmentAccessor = environmentAccessor;
            _appService = appService;
            _umbContextAccessor = umbContextAccessor;
            _localizedTextService = localizedTextService;
            _mapper = mapper;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        /// <inherit />
        public async Task<UpsertEnvironmentResponse> Upsert(UpsertEnvironmentRequest request)
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

            var response = new UpsertEnvironmentResponse();

            using (var umbContext = _umbContextAccessor.UmbracoContext)
            {
                var user = umbContext.Security.CurrentUser;

                if (environment.Key == default(Guid))
                {
                    try
                    {
                        response.Key = await _environmentAccessor.Create(environment);
                        _appService.WriteEnvironmentApps(response.Key);

                        var localizedMessage = _localizedTextService.Localize(
                            $"Shield.General/EnvironmentMessage",
                            new[]
                            {
                                user.Name,
                                "created",
                                environment.Name
                            });
                        _logger.Info<EnvironmentService>(localizedMessage);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error<EnvironmentService>(ex, "Error occurred inserting Environment");

                        response.ErrorCode = ErrorCode.EnviromentInsert;

                        return response;
                    }
                }
                else
                {
                    try
                    {
                        if (await _environmentAccessor.Update(environment))
                        {
                            response.Key = environment.Key;

                            _jobService.Unregister(environment);

                            var localizedMessage = _localizedTextService.Localize(
                                $"Shield.General/EnvironmentMessage",
                                new[]
                                {
                                    user.Name,
                                    "updated",
                                    environment.Name
                                });
                            _logger.Info<EnvironmentService>(localizedMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error<EnvironmentService>(ex, "Error occurred updating Environment");

                        response.ErrorCode = ErrorCode.EnvironmentUpdate;

                        return response;
                    }
                }
            }

            var appsResult = await _appService.GetApps(environment.Key);
            foreach (var app in appsResult.Apps)
            {
                _jobService.Register(environment, app.Key, app.Value);
            }

            _distributedCache.RefreshByJson(
                new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
                JsonConvert.SerializeObject(new EnvironmentCacheRefresherJsonModel(CacheRefreshType.Upsert, response.Key)));

            return response;
        }

        /// <inherit />
        public async Task<GetEnvironmentsResponse> Get()
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
        public async Task<GetEnvironmentResponse> Get(Guid key)
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
        public async Task<DeleteEnvironmentResponse> Delete(Guid key)
        {
            _distributedCache.RefreshByJson(
                new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
                JsonConvert.SerializeObject(new EnvironmentCacheRefresherJsonModel(CacheRefreshType.Remove, key)));

            var response = new DeleteEnvironmentResponse();

            try
            {
                response.Successful = await _environmentAccessor.Delete(key);
            }
            catch (Exception ex)
            {
                _logger.Error<EnvironmentService>(ex, "Error occurred deleting environment with {Key}", key);

                response.ErrorCode = ErrorCode.EnvrionmentDelete;
            }

            return response;
        }

        /// <inherit />
        public async Task<BaseResponse> SortEnvironments(UpdateEnvironmentsSortOrderRequest request)
        {
            if (request == null || request.Environments.None())
            {
                return new BaseResponse
                {
                    ErrorCode = ErrorCode.None
                };
            }

            var oldEnvironments = await Get();

            using (var umbContext = _umbContextAccessor.UmbracoContext)
            {
                var user = umbContext.Security.CurrentUser;

                foreach (var environment in request.Environments)
                {
                    if (!oldEnvironments.Environments.Any(x => x.Key.Equals(environment.Key) && !x.SortOrder.Equals(environment.SortOrder)))
                    {
                        continue;
                    }

                    try
                    {
                        if (await _environmentAccessor.Update(environment))
                        {
                            _jobService.Unregister(environment);

                            var localizedMessage = _localizedTextService.Localize(
                                $"Shield.General/EnvironmentMessage",
                                new[]
                                {
                                    user.Name,
                                    "updated",
                                    environment.Name
                                });
                            _logger.Info<EnvironmentService>(localizedMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error<EnvironmentService>(ex, "Error occurred updating Environment");

                        return new BaseResponse
                        {
                            ErrorCode = ErrorCode.EnvironmentUpdate
                        };
                    }

                    var appsResult = await _appService.GetApps(environment.Key);
                    foreach (var app in appsResult.Apps)
                    {
                        _jobService.Register(environment, app.Key, app.Value);
                    }
                }
            }

            _distributedCache.RefreshByJson(
                new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
                JsonConvert.SerializeObject(new EnvironmentCacheRefresherJsonModel(CacheRefreshType.ReOrder, Guid.Empty)));

            return new BaseResponse
            {
                ErrorCode = ErrorCode.None
            };
        }
    }
}
