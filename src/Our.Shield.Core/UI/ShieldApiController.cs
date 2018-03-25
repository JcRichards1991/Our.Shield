using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
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
    /// <example>Endpoint: /umbraco/backoffice/Shield/ShieldApi/{Action}</example>
    [PluginController(Constants.App.Alias)]
    public class ShieldApiController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Get configuration for each treenode
        /// </summary>
        /// <param name="id">jobId</param>
        /// <returns>All the info that the angular needs to render the view</returns>
        [HttpGet]
        public TreeView View(int id)
        {
            var environments = JobService.Instance.Environments.OrderBy(x => x.Key.SortOrder).ToDictionary(x => x.Key, v => v.Value);

            if (id == Constants.Dashboard.EnvironmentsDashboardId)
            {
                //  The environments Dashboard
                return new TreeView
                {
                    Description = "List of the different environments your Umbraco instance operates under",
                    Environments = environments.Keys,
                    Type = TreeViewType.Environments
                };
            }

            if (id == Constants.Tree.CreateEnvironmentId)
            {
                return new TreeView
                {
                    Environments = environments.Keys,
                    Type = TreeViewType.Environment,
                    Environment = new Models.Environment()
                };
            }

            foreach (var environment in environments)
            {
                if (id == environment.Key.Id)
                {
                    var apps = environment.Value.Select(x => new AppListingItem
                    {
                        Id = x.Id,
                        AppId = x.App.Id,
                        Name = x.App.Name,
                        Description = x.App.Description,
                        Icon = x.App.Icon,
                        Enable = x.ReadConfiguration().Enable
                    });

                    var appArray = apps as AppListingItem[] ?? apps.ToArray();
                    return new TreeView
                    {
                        Type = TreeViewType.Environment,
                        Name = environment.Key.Name,
                        Description = $"View apps for the {environment.Key.Name} environment",
                        Environment = environment.Key,
                        Environments = environments.Keys,
                        Apps = appArray.OrderBy(x => x.Name)
                    };
                }

                foreach (var job in environment.Value)
                {
                    if (id != job.Id)
                        continue;

                    var tabAttrs =
                    (job.App.GetType().GetCustomAttributes(typeof(AppTabAttribute), true) as
                         IEnumerable<AppTabAttribute> ?? new List<AppTabAttribute>()).ToList();
                        
                    //  TODO: Make tab captions localized
                    var tabs = new List<Tab>();
                    var appView = string.Empty;
                    foreach (var tabAttr in tabAttrs.OrderBy(x => x.SortOrder))
                    {
                        var tab = new Tab
                        {
                            View = tabAttr.FilePath,
                            Caption = tabAttr.Caption,
                            Id = tabAttr.SortOrder,
                            Active = tabAttr.SortOrder == 0
                        };

                        if (tabAttr is AppEditorAttribute appEditorAttr)
                        {
                            appView = appEditorAttr.AppView;
                        }

                        tabs.Add(tab);
                    }

                    return new TreeView
                    {
                        Type = TreeViewType.App,
                        Name = job.App.Name,
                        Description = job.App.Description,
                        Environments = environments.Keys,
                        Environment = environment.Key,
                        App = job.App,
                        AppView = appView,
                        Configuration = job.ReadConfiguration(),
                        Tabs = tabs
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="json"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderByDirection"></param>
        /// <returns></returns>
        [HttpGet]
        public JournalListing Journals(int? id, int page, string orderBy, string orderByDirection)
        {
            var environments = JobService.Instance.Environments;
            int totalPages;

            if (!id.HasValue)
            {
                return new JournalListing
                {
                    Journals = Persistance.Business.DbContext.Instance.Journal.Read<JournalMessage>(page, 200, out totalPages).Select(x =>
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

        private class DomainConverter : CustomCreationConverter<IDomain>
        {
            public override IDomain Create(Type objectType)
            {
                return new Domain();
            }
        }

        /// <summary>
        /// Save domains to an environment
        /// </summary>
        /// <param name="json">An Environment as json</param>
        /// <returns>True if save is successfully</returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public bool DeleteEnvironment(int id)
        {
            var environment = (Models.Environment)JobService.Instance.Environments.FirstOrDefault(x => x.Key.Id.Equals(id)).Key;

            return environment != null && EnvironmentService.Instance.Delete(environment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<IEnvironment> GetEnvironments()
        {
            return JobService.Instance.Environments.Select(x => x.Key).OrderBy(x => x.SortOrder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentsJson"></param>
        /// <returns></returns>
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
    }
}
