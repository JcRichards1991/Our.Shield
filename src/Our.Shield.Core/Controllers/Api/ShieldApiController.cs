using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Models.Requests;
using Our.Shield.Core.Models.Responses;
using Our.Shield.Core.Services;
using Our.Shield.Shared;
using Our.Shield.Shared.Extensions;
using System;
using System.Collections.Generic;
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
        /// <returns>Collection of environments</returns>
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
        /// <param name="environmentsJson">Collection of environments</param>
        /// <returns>True if successfully updated the sort order, otherwise, false</returns>
        [HttpPost]
        public bool SortEnvironments([FromBody] IEnumerable<JObject> environmentsJson)
        {
            //var json = environmentsJson.ToList();
            //if (!json.Any())
            //{
            //    return false;
            //}

            //var environments = json.Select(x => JsonConvert.DeserializeObject<Models.Environment>(x.ToString(), new DomainConverter()));
            //var oldEnvironments = JobService.Instance.Environments.Keys;

            //foreach (var environment in environments)
            //{
            //    if (!oldEnvironments.Any(x => x.Id.Equals(environment.Id) && !x.SortOrder.Equals(environment.SortOrder)))
            //        continue;

            //    if (!EnvironmentService.Instance.Write(environment))
            //    {
            //        return false;
            //    }

            //    if (!JobService.Instance.Unregister(environment))
            //    {
            //        return false;
            //    }

            //    JobService.Instance.Register(environment);
            //}

            //_distributedCache.RefreshByJson(
            //    Guid.Parse(Constants.DistributedCache.EnvironmentCacheRefresherId),
            //    GetJsonModel(new EnvironmentCacheRefresherJsonModel(Enums.CacheRefreshType.ReOrder, Guid.Empty)));

            return true;
        }

        /// <summary>
        /// Gets an environment by it's key
        /// </summary>
        /// <param name="key">The Key of the environment to fetch</param>
        /// <returns>The environment with the corresponding key</returns>
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
        /// <returns>true if successfully deleted, otherwise, false.</returns>
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
        /// <returns>The app with the corresponding key</returns>
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
        /// <param name="key">The key of the app to update</param>
        /// <param name="json">The new configuration as json</param>
        /// <returns>True if successfully updated, otherwise, false</returns>
        [HttpPost]
        public bool UpdateAppConfiguration(Guid key, [FromBody] JObject json)
        {
            throw new NotImplementedException();

            //if (json == null || key == Guid.Empty)
            //    return false;

            //var job = JobService.Instance.Job(key);

            //if (job == null)
            //{
            //    //  Invalid id
            //    return false;
            //}

            //if (!(json.ToObject(((Job)job).ConfigType) is IAppConfiguration configuration))
            //{
            //    return false;
            //}

            //configuration.Enable = json.GetValue(nameof(IAppConfiguration.Enable), StringComparison.InvariantCultureIgnoreCase).Value<bool>();

            //job.WriteJournal(new JournalMessage($"{Security.CurrentUser.Name} has updated the configuration"));

            //if (job.WriteConfiguration(configuration))
            //{
            //    _distributedCache.RefreshByJson(
            //        Guid.Parse(Constants.DistributedCache.ConfigurationCacheRefresherId),
            //        GetJsonModel(new ConfigurationCacheRefresherJsonModel(Enums.CacheRefreshType.Write, key)));

            //    return true;
            //}

            //return false;
        }

        /// <summary>
        /// Gets the journals based on the parameters passed in
        /// </summary>
        /// <param name="method">The level of which to return journals.
        /// Environments: Returns all journals for all Environments &amp; Apps;
        /// Environment: Returns all Journals for a given Environment &amp; children Apps;
        /// App: Returns only the Journals for a given app</param>
        /// <param name="id">Ignored if method is Environments. Guid Key of the Environment or App to retrieve Journals for</param>
        /// <param name="page">The page of Journals to retrieve</param>
        /// <param name="orderBy">The type to sort the Journals by (currently ignored)</param>
        /// <param name="orderByDirection">The order in which to sort by. acs or desc</param>
        /// <returns>Collection of Journals based on the parameters passed in</returns>
        [HttpGet]
        public IHttpActionResult Journals(string method, string id, int page, string orderBy, string orderByDirection)
        {
            throw new NotImplementedException();

            //var environments = JobService.Instance.Environments;

            //switch (method)
            //{
            //    case "Environments":
            //        throw new NotImplementedException();
            //        //return new JournalListing
            //        //{
            //        //    Journals = DbContext.Instance.Journal.Read<JournalMessage>(page, 200, out int totalPages).Select(x =>
            //        //    {
            //        //        var e = environments.FirstOrDefault(ev => ev.Key.Id == x.EnvironmentId);
            //        //        return new JournalListingItem
            //        //        {
            //        //            DateStamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
            //        //            App = new AppListingItem(e.Value.FirstOrDefault(j => j.App.Id == x.AppId)),
            //        //            Environment = e.Key,
            //        //            Message = x.Message
            //        //        };
            //        //    }),
            //        //    TotalPages = totalPages
            //        //};

            //    case "Environment":
            //        throw new NotImplementedException();
            //        //var envId = Guid.Parse(id);
            //        //var environment = environments.FirstOrDefault(x => x.Key.Key == envId);
            //        //return new JournalListing
            //        //{
            //        //    Journals = EnvironmentService.Instance
            //        //        .JournalListing<JournalMessage>(environment.Key.Id, page, 100, out var totalPages)
            //        //        .Select(x => new JournalListingItem
            //        //        {
            //        //            DateStamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
            //        //            App = new AppListingItem(environment.Value.FirstOrDefault(j => j.App.Id == x.AppId)),
            //        //            Environment = environment.Key,
            //        //            Message = x.Message
            //        //        }),
            //        //    TotalPages = totalPages
            //        //};

            //    case "App":
            //        throw new NotImplementedException();
            //        //var appId = Guid.Parse(id);
            //        //environment = environments.FirstOrDefault(x => x.Value.Any(j => j.Key == appId));
            //        //var job = environment.Value.First(x => x.Key == appId);
            //        //return new JournalListing
            //        //{
            //        //    Journals = job.ListJournals<JournalMessage>(page, 50, out totalPages).Select(x => new JournalListingItem
            //        //    {
            //        //        DateStamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
            //        //        App = new AppListingItem(job),
            //        //        Environment = environment.Key,
            //        //        Message = x.Message
            //        //    }),
            //        //    TotalPages = totalPages
            //        //};

            //    default:
            //        return null;
            //}
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
