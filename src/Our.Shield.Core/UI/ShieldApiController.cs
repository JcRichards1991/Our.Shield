using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Persistence.Business;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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
        [HttpPost]
        public bool DeleteEnvironment(Guid key)
        {
            var environment = (Models.Environment)JobService.Instance.Environments.FirstOrDefault(x => x.Key.Key == key).Key;
            return environment != null && EnvironmentService.Instance.Delete(environment);
        }

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

        [HttpGet]
        public IEnumerable<IEnvironment> GetEnvironments()
        {
            return JobService.Instance.Environments.Select(x => x.Key).OrderBy(x => x.SortOrder);
        }

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
            return true;
        }

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

            if (Security.CurrentUser == null)
            {
                return job.WriteConfiguration(configuration);
            }

            var user = Security.CurrentUser;
            job.WriteJournal(new JournalMessage($"{user.Name} has updated the configuration"));

            return job.WriteConfiguration(configuration);
        }

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

            return EnvironmentService.Instance.Write(environment);
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
