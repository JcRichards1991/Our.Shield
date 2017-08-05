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
                var journals = Persistance.Business.DbContext.Instance.Journal.Read<JournalMessage>(1, 200, out totalPages);
                var apps = environments.First().Value.Select(x => x.App);

                //  Is the environments node
                return new TreeView
                {
                    Type = TreeView.TreeViewType.Environments,
                    Name = "Environments",
                    Description = "List of the different environments your Umbraco instance operates under",
                    Environments = environments.Keys,
                    JournalListing = new JournalListing
                    {
                        Journals = journals.Select(x => new JournalListingItem
                        {
                            Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                            App = apps.FirstOrDefault(a => a.Id == x.AppId).Name,
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
                    var journals = environment.Key.JournalListing<JournalMessage>(1, 100, out totalPages);
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
                                Environment = environment.Key.Name,
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
                                    Environment = environment.Key.Name,
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
        /// <param name="json">An Environment as json</param>
        /// <returns>True if save is successfully</returns>
        [HttpPost]
        public bool Environment([FromBody] JObject json)
        {
            if (json == null)
            {
                //  json is invalid
                return false;
            }

            var environment = json.ToObject<Environment>();

            if(environment.WriteEnvironment())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public bool DeleteEnvironment(int id)
        {
            var environment = (Environment) Operation.JobService.Instance.Environments.FirstOrDefault(x => x.Key.Id.Equals(id)).Key;

            return environment.DeleteEnvironment();
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
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public JournalListing Journals(int id, int page)
        {
            var environments = Operation.JobService.Instance.Environments;
            var apps = environments.First().Value.Select(x => x.App);
            int totalPages = 1;

            if (id == Constants.Tree.EnvironmentsRootId)
            {
                return new JournalListing
                {
                    Journals = Persistance.Business.DbContext.Instance.Journal.Read<JournalMessage>(page, 200, out totalPages).Select(x => new JournalListingItem
                    {
                        Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                        App = apps.FirstOrDefault(a => a.Id == x.AppId).Name,
                        Environment = environments.FirstOrDefault(e => e.Key.Id == x.EnvironmentId).Key.Name,
                        Message = x.Message
                    }),
                    TotalPages = totalPages
                };
            }
            else
            {
                foreach (var environment in environments)
                {
                    if (id == environment.Key.Id)
                    {
                        return new JournalListing
                        {
                            Journals = environment.Key.JournalListing<JournalMessage>(page, 100, out totalPages).Select(x => new JournalListingItem
                            {
                                Datestamp = x.Datestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                                App = apps.FirstOrDefault(a => a.Id == x.AppId).Name,
                                Environment = environment.Key.Name,
                                Message = x.Message
                            }),
                            TotalPages = totalPages
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
                                    App = job.App.Name,
                                    Environment = environment.Key.Name,
                                    Message = x.Message
                                }),
                                TotalPages = totalPages
                            };
                        }
                    }
                }
            }

            return null;
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
