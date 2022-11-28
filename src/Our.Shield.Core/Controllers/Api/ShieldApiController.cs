using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models.Requests;
using Our.Shield.Core.Models.Responses;
using Our.Shield.Core.Services;
using Our.Shield.Shared;
using Our.Shield.Shared.Extensions;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using UmbConstants = Umbraco.Core.Constants;

namespace Our.Shield.Core.Controllers.Api
{
    /// <summary>
    /// API Controller for Shield
    /// </summary>
    /// <example>
    /// Endpoint: /Umbraco/BackOffice/Shield/ShieldApi/{Action}
    /// </example>
    [PluginController(Constants.App.Alias)]
    [UmbracoApplicationAuthorize(UmbConstants.Applications.Settings)]
    public class ShieldApiController : UmbracoAuthorizedApiController
    {
        private readonly IEnvironmentService _environmentService;

        private readonly IAppService _appService;

        /// <summary>
        /// Initializes a new instance of <see cref="ShieldApiController"/> class
        /// </summary>
        /// <param name="globalSettings"><see cref="IGlobalSettings"/></param>
        /// <param name="umbContextAccessor"><see cref="IUmbracoContextAccessor"/></param>
        /// <param name="sqlContext"><see cref="ISqlContext"/></param>
        /// <param name="serviceContext"><see cref="ServiceContext"/></param>
        /// <param name="appCaches"><see cref="AppCaches"/></param>
        /// <param name="profilingLogger"><see cref="IProfilingLogger"/></param>
        /// <param name="runtimeState"><see cref="IRuntimeState"/></param>
        /// <param name="umbHelper"><see cref="UmbracoHelper"/></param>
        /// <param name="environmentService"><see cref="IEnvironmentService"/>.</param>
        /// <param name="appService"><see cref="IAppService"/></param>
        public ShieldApiController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbContextAccessor,
            ISqlContext sqlContext,
            ServiceContext serviceContext,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IRuntimeState runtimeState,
            UmbracoHelper umbHelper,
            IEnvironmentService environmentService,
            IAppService appService)
            : base(globalSettings, umbContextAccessor, sqlContext, serviceContext, appCaches, profilingLogger, runtimeState, umbHelper)
        {
            GuardClauses.NotNull(environmentService, nameof(environmentService));
            GuardClauses.NotNull(appService, nameof(appService));

            _environmentService = environmentService;
            _appService = appService;
        }

        /// <summary>
        /// Gets all environments available in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetEnvironments()
        {
            var response = await _environmentService.Get();

            return ApiResponse(
                response,
                response.HasError() ? HttpStatusCode.BadRequest : HttpStatusCode.OK);
        }

        /// <summary>
        /// Sorts the environments
        /// </summary>
        /// <param name="request"><see cref="UpdateEnvironmentsSortOrderRequest"/></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> SortEnvironments(UpdateEnvironmentsSortOrderRequest request)
        {
            var response = await _environmentService.SortEnvironments(request);

            return ApiResponse(
                response,
                response.HasError() ? HttpStatusCode.BadRequest : HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets an environment by it's key
        /// </summary>
        /// <param name="key">The Key of the environment to fetch</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetEnvironment(Guid key)
        {
            var response = await _environmentService.Get(key);

            return response.HasError()
                ? ApiResponse(response, HttpStatusCode.BadGateway)
                : response.Environment == null
                    ? StatusCode(HttpStatusCode.NoContent)
                    : ApiResponse(response);
        }

        /// <summary>
        /// Updates an environment in the database
        /// </summary>
        /// <param name="request"><see cref="UpsertEnvironmentRequest"/>.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> UpsertEnvironment(UpsertEnvironmentRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _environmentService.Upsert(request);

            return ApiResponse(
                response,
                response.HasError()
                    ? HttpStatusCode.BadRequest
                    : HttpStatusCode.OK);
        }

        /// <summary>
        /// Deletes an environment via its key
        /// </summary>
        /// <param name="key">The key of the environment to delete</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteEnvironment(Guid key)
        {
            var response = await _environmentService.Delete(key);

            return ApiResponse(
                response,
                response.HasError() ? HttpStatusCode.BadRequest : HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets all apps for an environment
        /// </summary>
        /// <param name="environmentKey">The Id of the environment to fetch the apps for</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetEnvironmentApps(Guid environmentKey)
        {
            var response = await _appService.GetApps(environmentKey);

            return response.HasError()
                ? ApiResponse(response, HttpStatusCode.BadGateway)
                : response.Apps.None()
                    ? StatusCode(HttpStatusCode.NoContent)
                    : ApiResponse(response);
        }

        /// <summary>
        /// Gets an app by it's key
        /// </summary>
        /// <param name="key">The key of the app to fetch</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetApp(Guid key)
        {
            var response = await _appService.GetApp(key);

            return response.HasError()
                ? ApiResponse(response, HttpStatusCode.BadGateway)
                : ApiResponse(response);
        }

        /// <summary>
        /// Updates an apps configuration to the database
        /// </summary>
        /// <param name="request"><see cref="UpdateAppConfigurationRequest"/></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> UpdateAppConfiguration(UpdateAppConfigurationRequest request)
        {
            var response = await _appService.UpdateAppConfiguration(request);

            return response.HasError()
                ? ApiResponse(response, HttpStatusCode.BadGateway)
                : ApiResponse(response);
        }

        private IHttpActionResult ApiResponse<T>(T response, HttpStatusCode statusCode = HttpStatusCode.OK)
            where T : BaseResponse
        {
            return ResponseMessage(
                new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(
                        JsonConvert.SerializeObject(
                            response,
                            new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }),
                        Encoding.UTF8,
                        "application/json")
                });
        }
    }
}
