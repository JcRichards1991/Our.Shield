namespace Our.Shield.Core.UI
{
    using Models;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;

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
            var environments = Operation.JobService.Instance.Environments;
            int totalPages = 1;

            if (id == Constants.Tree.EnvironmentsRootId)
            {
                var journals = Persistance.Business.DbContext.Instance.Journal.FetchAll<JournalMessage>(1, 50, out totalPages);
                var apps = environments.SelectMany(x => x.Value).Select(x => new AppListingItem
                {
                    Id = x.Id,
                    AppId = x.App.Id,
                    Name = x.App.Name,
                    Description = x.App.Description,
                    Icon = x.App.Icon,
                    Enable = x.ReadConfiguration().Enable
                });

                //  Is the environments node
                return new TreeView
                {
                    Type = TreeView.TreeViewType.Environments,
                    Name = "Environments",
                    Description = "List of the different environments your Umbraco instance operates under",
                    Environments = environments.Keys,
                    Apps = apps,
                    JournalListing = new JournalListing
                    {
                        Journals = journals.Select(x => new JournalListingItem
                        {
                            Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                            App = apps.FirstOrDefault(a => a.AppId == x.AppId).Name,
                            Environment = environments.FirstOrDefault(e => e.Key.Id == x.EnvironmentId).Key.Name,
                            Message = x.Message
                        }),
                        TotalPages = totalPages
                    }
                };
            }

            foreach (var environment in environments)
            {
                if (id == environment.Key.Id)
                {
                    var journals = environment.Key.JournalListing<JournalMessage>(1, 50, out totalPages);
                    var apps = environment.Value.Select(x => new AppListingItem
                    {
                        Id = x.Id,
                        AppId = x.App.Id,
                        Name = x.App.Name,
                        Description = x.App.Description,
                        Icon = x.App.Icon,
                        Enable = x.ReadConfiguration().Enable
                    });

                    return new TreeView
                    {
                        Type = TreeView.TreeViewType.Environment,
                        Name = environment.Key.Name,
                        Description = $"View apps for the {environment.Key.Name} environment",
                        Environments = environments.Keys,
                        Environment = environment.Key,
                        Apps = apps,
                        JournalListing = new JournalListing
                        {
                            Journals = journals.Select(x => new JournalListingItem
                            {
                                Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                                App = apps.FirstOrDefault(a => a.AppId == x.AppId).Name,
                                Environment = environments.FirstOrDefault(e => e.Key.Id == x.EnvironmentId).Key.Name,
                                Message = x.Message
                            }),
                            TotalPages = totalPages
                        }
                    };
                }

                foreach (var job in environment.Value)
                {
                    if (id == job.Id)
                    {
                        var appAssests = new AppAssest
                        {
                            View = job.App.GetType().GetCustomAttribute<AppEditorAttribute>()?.FilePath ?? null,

                            Stylesheets = job.App.GetType().GetCustomAttributes<AppAssetAttribute>()
                                .Where(x => x.AssetType == ClientDependency.Core.ClientDependencyType.Css)
                                .Select(x => x.FilePath),

                            Scripts = job.App.GetType().GetCustomAttributes<AppAssetAttribute>()
                                .Where(x => x.AssetType == ClientDependency.Core.ClientDependencyType.Javascript)
                                .Select(x => x.FilePath)
                        };
                        
                        var journals = job.ListJournals<JournalMessage>(1, 50, out totalPages);

                        return new TreeView
                        {
                            Type = TreeView.TreeViewType.App,
                            Name = job.App.Name,
                            Description = job.App.Description,
                            Environments = environments.Keys,
                            Environment = environment.Key,
                            App = job.App,
                            Configuration = job.ReadConfiguration(),
                            AppAssests = appAssests,
                            JournalListing = new JournalListing
                            {
                                Journals = journals.Select(x => new JournalListingItem
                                {
                                    Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                                    App = job.App.Name,
                                    Environment = environments.FirstOrDefault(e => e.Key.Id == x.EnvironmentId).Key.Name,
                                    Message = x.Message
                                }),
                                TotalPages = totalPages,
                            }
                        };
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Save domains to an environment
        /// </summary>
        /// <param name="id">jobId</param>
        /// <param name="domains">The list of new domains you wish to save</param>
        /// <returns>True if save is successfully</returns>
        [HttpPost]
        public bool Domains(int id, [FromBody] IEnumerable<IDomain> domains)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        [HttpPost]
        public new bool Configuration(int id, [FromBody] JObject json)
        {
            if (json == null)
            {
                //  json is invalid
                return false;
            }

            var job = Operation.JobService.Instance.Job(id);
            if (job == null)
            {
                //  Invalid id
                return false;
            }
            var configuration = json.ToObject(((Job)job).ConfigType) as IConfiguration;
            configuration.Enable = json.GetValue(nameof(IConfiguration.Enable), System.StringComparison.InvariantCultureIgnoreCase).Value<bool>();
            
            if (Security.CurrentUser != null)
            {
                var user = Security.CurrentUser;
                job.WriteJournal(new JournalMessage($"{user.Name} has updated the configuration"));
            }

            return job.WriteConfiguration(configuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="appId"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<JournalListingItem> Journals(int environmentId, int appId, int page, int itemsPerPage)
        {
            var job = Operation.JobService.Instance.Job(appId);

            int totalPages = 1;
            var journals = job.ListJournals<JournalMessage>(page, itemsPerPage, out totalPages);

            return journals.Select(x => new JournalListingItem
            {
                Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                App = job.App.Name,
                //Environment = environments.FirstOrDefault(e => e.Key.Id == x.EnvironmentId).Key.Name,
                Message = x.Message,
            });
        }

        /// <summary>
        /// Gets a collection of the Shield App Ids that are installed
        /// </summary>
        /// <returns>Collection of the Shield App ids</returns>
        [HttpGet]
        public IEnumerable<string> AppIds()
        {
            var appIds = new List<string>();
            var env = Operation.JobService.Instance.Environments.FirstOrDefault();

            foreach (var job in env.Value)
            {
                appIds.Add(job.App.Id);
            }

            return appIds;
        }
    }
}
