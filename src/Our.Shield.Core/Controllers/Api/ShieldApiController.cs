using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Models;
using Our.Shield.Core.Models.Requests;
using Our.Shield.Core.Models.Responses;
using Our.Shield.Core.Services;
using Our.Shield.Shared;
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
    /// API Controller for the Umbraco Access area of the custom section
    /// </summary>
    /// <example>
    /// Endpoint: /Umbraco/BackOffice/Shield/ShieldApi/{Action}
    /// </example>
    [PluginController(Constants.App.Alias)]
    [UmbracoApplicationAuthorize(UmbConstants.Applications.Settings)]
    public class ShieldApiController : UmbracoAuthorizedApiController
    {
        private readonly IEnvironmentService _environmentService;

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
        /// /// <param name="environmentService"><see cref="IEnvironmentService"/>.</param>
        public ShieldApiController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbContextAccessor,
            ISqlContext sqlContext,
            ServiceContext serviceContext,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IRuntimeState runtimeState,
            UmbracoHelper umbHelper,
            IEnvironmentService environmentService)
            : base(globalSettings, umbContextAccessor, sqlContext, serviceContext, appCaches, profilingLogger, runtimeState, umbHelper)
        {
            GuardClauses.NotNull(environmentService, nameof(environmentService));

            _environmentService = environmentService;
        }

        /// <summary>
        /// Get all environments available in the system
        /// </summary>
        /// <returns>Collection of environments</returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetEnvironments()
        {
            return ApiResponse(await _environmentService.Get());
        }

        /// <summary>
        /// Deletes an environment via its key
        /// </summary>
        /// <param name="key">The key of the environment to delete</param>
        /// <returns>true if successfully deleted, otherwise, false.</returns>
        [HttpPost]
        public async Task<DeleteEnvironmentResponse> DeleteEnvironment(Guid key)
        {
            var response = _environmentService.Delete(key);

            throw new NotImplementedException();
            //var environment = (Models.Environment)JobService.Instance.Environments.FirstOrDefault(x => x.Key.Key == key).Key;

            //if (environment != null && await _environmentService.Delete(environment))
            //{
            //    _distributedCache.RefreshByJson(
            //        new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
            //        GetJsonModel(new EnvironmentCacheRefresherJsonModel(Enums.CacheRefreshType.Remove, key)));

            //    return true;
            //}

            //return false;
        }

        /// <summary>
        /// Gets an app by it's key
        /// </summary>
        /// <param name="key">The key of the app to fetch</param>
        /// <returns>The app with the corresponding key</returns>
        [HttpGet]
        public IHttpActionResult GetApp(Guid key)
        {
            throw new NotImplementedException();

            //var environment = JobService.Instance.Environments.FirstOrDefault(x => x.Value.Any(y => y.Key == key));

            //if (environment.Key == null)
            //    return null;

            //var job = environment.Value.First(x => x.Key == key);

            //var tabAttrs = (job.App.GetType().GetCustomAttributes(typeof(AppTabAttribute), true) as IEnumerable<AppTabAttribute> ?? new List<AppTabAttribute>()).ToList();

            ////  TODO: Make tab captions localized
            //var tabs = new List<ITab>();
            //foreach (var tabAttr in tabAttrs.OrderBy(x => x.SortOrder))
            //{
            //    if (tabAttr is AppEditorAttribute appEditorAttr)
            //    {
            //        tabs.Add(new AppConfigTab(appEditorAttr));
            //        continue;
            //    }

            //    tabs.Add(new Tab(tabAttr));
            //}

            //return new AppApiResponseModel(job)
            //{
            //    //Environments = environments.Keys,
            //    Environment = environment.Key,
            //    Configuration = job.ReadConfiguration(),
            //    Tabs = tabs
            //};
        }

        /// <summary>
        /// Gets an environment by it's key
        /// </summary>
        /// <param name="key">The Key of the environment to fetch</param>
        /// <returns>The environment with the corresponding key</returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetEnvironment(Guid key)
        {
            var environment = await _environmentService.Get(key);

            if (environment == null)
            {
                return NotFound();
            }

            return ApiResponse(environment);
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
        /// <param name="orderBy">The type to sort the Journals by(currently ignored)</param>
        /// <param name="orderByDirection">The order in which to order. acs or desc</param>
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
        /// Updates an apps configuration to the database
        /// </summary>
        /// <param name="key">The key of the app to update</param>
        /// <param name="json">The new configuration as json</param>
        /// <returns>True if successfully updated, otherwise, false</returns>
        [HttpPost]
        public bool WriteConfiguration(Guid key, [FromBody] JObject json)
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

        private IHttpActionResult ApiResponse<T>(
            T response,
            HttpStatusCode statusCode = HttpStatusCode.OK)
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

        private class DomainConverter : CustomCreationConverter<IDomain>
        {
            public override IDomain Create(Type objectType)
            {
                return new Domain();
            }
        }
    }
}
