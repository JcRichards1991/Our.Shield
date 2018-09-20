using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Our.Shield.Core.Models;
using Our.Shield.Core.Persistence.Business;
using Our.Shield.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.UI.WebControls;
using Our.Shield.Core.Attributes;
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
        public bool DeleteEnvironment(int id)
        {
            var environment = (Models.Environment)JobService.Instance.Environments.FirstOrDefault(x => x.Key.Id.Equals(id)).Key;
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
        public JournalListing Journals(int? id, int page, string orderBy, string orderByDirection)
        {
            var environments = JobService.Instance.Environments;
            int totalPages;

            if (!id.HasValue)
            {
                return new JournalListing
                {
                    Journals = DbContext.Instance.Journal.Read<JournalMessage>(page, 200, out totalPages).Select(x =>
                    {
                        var environment = environments.FirstOrDefault(e => e.Key.Id == x.EnvironmentId);
                        return new JournalListingItem
                        {
                            Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                            App = new AppListingItem(environment.Value.FirstOrDefault(j => j.App.Id == x.AppId)),
                            Environment = environment.Key,
                            Message = x.Message
                        };
                    }),
                    TotalPages = totalPages,
                    Type = TreeViewType.Environments
                };
            }

            foreach (var environment in environments)
            {
                if (id == environment.Key.Id)
                {
                    return new JournalListing
                    {
                        Journals = EnvironmentService.Instance.JournalListing<JournalMessage>(id.Value, page, 100, out totalPages).Select(x => new JournalListingItem
                        {
                            Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                            App = new AppListingItem(environment.Value.FirstOrDefault(j => j.App.Id == x.AppId)),
                            Environment = environment.Key,
                            Message = x.Message
                        }),
                        TotalPages = totalPages,
                        Type = TreeViewType.Environment
                    };
                }

                foreach (var job in environment.Value)
                {
                    if (id == job.Id)
                    {
                        return new JournalListing
                        {
                            Journals = job.ListJournals<JournalMessage>(page, 50, out totalPages).Select(x => new JournalListingItem
                            {
                                Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                                App = new AppListingItem(job),
                                Environment = environment.Key,
                                Message = x.Message
                            }),
                            TotalPages = totalPages,
                            Type = TreeViewType.App
                        };
                    }
                }
            }

            return null;
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
        public bool WriteConfiguration(int id, [FromBody] JObject json)
        {
            if (json == null)
            {
                //  json is invalid
                return false;
            }

            var job = JobService.Instance.Job(id);
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
