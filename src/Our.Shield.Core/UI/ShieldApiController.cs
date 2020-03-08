using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Models.CacheRefresherJson;
using Our.Shield.Core.Persistence.Business;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Umbraco.Web.Cache;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Our.Shield.Core.UI
{
    /// <inheritdoc />
    /// <summary>
    /// Api Controller for the Umbraco Access area of the custom section
    /// </summary>
    /// <example>
    /// Endpoint: /Umbraco/BackOffice/Shield/ShieldApi/{Action}
    /// </example>
    [PluginController(Constants.App.Alias)]
    public class ShieldApiController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Deletes an environment via its key
        /// </summary>
        /// <param name="key">The key of the environment to delete</param>
        /// <returns>true if successfully deleted, otherwise, false.</returns>
        [HttpPost]
        public bool DeleteEnvironment(Guid key)
        {
            var environment = (Models.Environment)JobService.Instance.Environments.FirstOrDefault(x => x.Key.Key == key).Key;
            
            if (environment != null && EnvironmentService.Instance.Delete(environment))
            {
                DistributedCache.Instance.RefreshByJson(
                    new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
                    GetJsonModel(new EnvironmentCacheRefresherJsonModel(Enums.CacheRefreshType.Remove, key)));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an app by it's key
        /// </summary>
        /// <param name="key">The key of the app to fetch</param>
        /// <returns>The app with the corresponding key</returns>
        [HttpGet]
        public AppApiResponseModel GetApp(Guid key)
        {
            var environment = JobService.Instance.Environments.FirstOrDefault(x => x.Value.Any(y => y.Key == key));

            if (environment.Key == null)
                return null;

            var job = environment.Value.First(x => x.Key == key);

            var tabAttrs = (job.App.GetType().GetCustomAttributes(typeof(AppTabAttribute), true) as IEnumerable<AppTabAttribute> ?? new List<AppTabAttribute>()).ToList();

            //  TODO: Make tab captions localized
            var tabs = new List<ITab>();
            foreach (var tabAttr in tabAttrs.OrderBy(x => x.SortOrder))
            {
                if (tabAttr is AppEditorAttribute appEditorAttr)
                {
                    tabs.Add(new AppConfigTab(appEditorAttr));
                    continue;
                }

                tabs.Add(new Tab(tabAttr));
            }

            return new AppApiResponseModel(job)
            {
                //Environments = environments.Keys,
                Environment = environment.Key,
                Configuration = job.ReadConfiguration(),
                Tabs = tabs
            };
        }

        /// <summary>
        /// Gets an environment by it's key
        /// </summary>
        /// <param name="key">The Key of the environment to fetch</param>
        /// <returns>The environment with the corresponding key</returns>
        [HttpGet]
        public EnvironmentApiResponseModel GetEnvironment(Guid key)
        {
            var environment = JobService.Instance.Environments.FirstOrDefault(x => x.Key.Key == key);

            if (environment.Key == null)
                return null;

            return new EnvironmentApiResponseModel(environment.Key)
            {
                Description = $"View apps for {environment.Key.Name} environment",
                Apps = environment.Value.Select(x => new AppListingItem(x)).OrderBy(x => x.Name).ToArray()
            };
        }

        /// <summary>
        /// Get all environments available in the system
        /// </summary>
        /// <returns>Collection of environments</returns>
        [HttpGet]
        public IEnumerable<IEnvironment> GetEnvironments()
        {
            return JobService.Instance.Environments.Select(x => x.Key).OrderBy(x => x.SortOrder);
        }

        /// <summary>
        /// Gets the journals based on the parameters passed in
        /// </summary>
        /// <param name="method">The level of which to return journals.
        /// Environments: Returns all journals for all Environments & Apps;
        /// Environment: Returns all Journals for a given Environment & children Apps;
        /// App: Returns only the Journals for a given app</param>
        /// <param name="id">Ignored if method is Environments. Guid Key of the Environment or App to retrieve Journals for</param>
        /// <param name="page">The page of Journals to retrieve</param>
        /// <param name="orderBy">The type to sort the Journals by(currently ignored)</param>
        /// <param name="orderByDirection">The order in which to order. acs or desc</param>
        /// <returns>Collection of Journals based on the parameters passed in</returns>
        [HttpGet]
        public JournalListing Journals(string method, string id, int page, string orderBy, string orderByDirection)
        {
            var environments = JobService.Instance.Environments;

            switch (method)
            {
                case "Environments":
                    return new JournalListing
                    {
                        Journals = DbContext.Instance.Journal.Read<JournalMessage>(page, 200, out int totalPages).Select(x =>
                        {
                            var e = environments.FirstOrDefault(ev => ev.Key.Id == x.EnvironmentId);
                            return new JournalListingItem
                            {
                                DateStamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                                App = new AppListingItem(e.Value.FirstOrDefault(j => j.App.Id == x.AppId)),
                                Environment = e.Key,
                                Message = x.Message
                            };
                        }),
                        TotalPages = totalPages
                    };

                case "Environment":
                    var envId = Guid.Parse(id);
                    var environment = environments.FirstOrDefault(x => x.Key.Key == envId);
                    return new JournalListing
                    {
                        Journals = EnvironmentService.Instance
                            .JournalListing<JournalMessage>(environment.Key.Id, page, 100, out totalPages)
                            .Select(x => new JournalListingItem
                            {
                                DateStamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                                App = new AppListingItem(environment.Value.FirstOrDefault(j => j.App.Id == x.AppId)),
                                Environment = environment.Key,
                                Message = x.Message
                            }),
                        TotalPages = totalPages
                    };

                case "App":
                    var appId = Guid.Parse(id);
                    environment = environments.FirstOrDefault(x => x.Value.Any(j => j.Key == appId));
                    var job = environment.Value.First(x => x.Key == appId);
                    return new JournalListing
                    {
                        Journals = job.ListJournals<JournalMessage>(page, 50, out totalPages).Select(x => new JournalListingItem
                        {
                            DateStamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                            App = new AppListingItem(job),
                            Environment = environment.Key,
                            Message = x.Message
                        }),
                        TotalPages = totalPages
                    };

                default:
                    return null;
            }
        }

        /// <summary>
        /// Sorts the environments
        /// </summary>
        /// <param name="environmentsJson">Collection of environments</param>
        /// <returns>True if successfully updated the sort order, otherwise, false</returns>
        [HttpPost]
        public bool SortEnvironments([FromBody] IEnumerable<JObject> environmentsJson)
        {
            var json = environmentsJson.ToList();
            if (!json.Any())
            {
                return false;
            }

            var environments = json.Select(x => JsonConvert.DeserializeObject<Models.Environment>(x.ToString(), new DomainConverter()));
            var oldEnvironments = JobService.Instance.Environments.Keys;

            foreach (var environment in environments)
            {
                if (!oldEnvironments.Any(x => x.Id.Equals(environment.Id) && !x.SortOrder.Equals(environment.SortOrder)))
                    continue;

                if (!EnvironmentService.Instance.Write(environment))
                {
                    return false;
                }

                if (!JobService.Instance.Unregister(environment))
                {
                    return false;
                }

                JobService.Instance.Register(environment);
            }

            DistributedCache.Instance.RefreshByJson(
                Guid.Parse(Constants.DistributedCache.EnvironmentCacheRefresherId),
                GetJsonModel(new EnvironmentCacheRefresherJsonModel(Enums.CacheRefreshType.ReOrder, Guid.Empty)));

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
            if (json == null || key == Guid.Empty)
                return false;

            var job = JobService.Instance.Job(key);

            if (job == null)
            {
                //  Invalid id
                return false;
            }

            if (!(json.ToObject(((Job)job).ConfigType) is IAppConfiguration configuration))
            {
                return false;
            }

            configuration.Enable = json.GetValue(nameof(IAppConfiguration.Enable), StringComparison.InvariantCultureIgnoreCase).Value<bool>();

            job.WriteJournal(new JournalMessage($"{Security.CurrentUser.Name} has updated the configuration"));

            if (job.WriteConfiguration(configuration))
            {
                DistributedCache.Instance.RefreshByJson(
                    Guid.Parse(Constants.DistributedCache.ConfigurationCacheRefresherId),
                    GetJsonModel(new ConfigurationCacheRefresherJsonModel(Enums.CacheRefreshType.Write, key)));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates an environment in the database
        /// </summary>
        /// <param name="json">the environment new settings as json</param>
        /// <returns></returns>
        [HttpPost]
        public bool WriteEnvironment([FromBody] JObject json)
        {
            if (json == null)
            {
                //  json is invalid
                return false;
            }

            var environment = JsonConvert.DeserializeObject<Models.Environment>(json.ToString(), new DomainConverter());

            var environments = JobService
                .Instance
                .Environments
                .Select(x => x.Key)
                .Where(x => x.SortOrder != Constants.Tree.DefaultEnvironmentSortOrder)
                .ToList();

            if (!environments.Any(x => x.Key == environment.Key))
            {
                environment.SortOrder = environments.Any()
                    ? environments.Max(x => x.SortOrder) + 1
                    : 0;
            }

            if (EnvironmentService.Instance.Write(environment))
            {
                DistributedCache.Instance.RefreshByJson(
                    new Guid(Constants.DistributedCache.EnvironmentCacheRefresherId),
                    GetJsonModel(new EnvironmentCacheRefresherJsonModel(Enums.CacheRefreshType.Write, environment.Key)));

                return true;
            }

            return false;
        }

        private string GetJsonModel(ICacheRefreshJsonModel jsonModel)
        {
            return JsonConvert.SerializeObject(jsonModel);
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
