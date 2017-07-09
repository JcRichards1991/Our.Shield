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
    /// <example>
    /// Endpoint: /umbraco/backoffice/Shield/ShieldApi/{Action}
    /// </example>
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
            if (id == Constants.Tree.EnvironmentsRootId)
            {
                //  Is the environments node
                return new TreeView
                {
                    Type = UI.TreeView.TreeViewType.Environments,
                    Name = "Environments",
                    Description = "List of the different environments your Umbraco instance operates under",
                    Environments = environments.Keys
                };
            }

            foreach (var environment in environments)
            {
                if (id == environment.Key.Id)
                {
                    return new TreeView
                    {
                        Type = UI.TreeView.TreeViewType.Environment,
                        Name = environment.Key.Name,
                        Description = $"View apps for the {environment.Key.Name} environment",
                        Environments = environments.Keys,
                        Environment = environment.Key,
                        Apps = environment.Value.Select(x => new KeyValuePair<int, IApp>(x.Id, x.App))
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
                        
                        return new TreeView
                        {
                            Type = UI.TreeView.TreeViewType.App,
                            Name = job.App.Name,
                            Description = job.App.Description,
                            Environments = environments.Keys,
                            Environment = environment.Key,
                            App = job.App,
                            Configuration = job.ReadConfiguration(),
                            AppAssests = appAssests
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
            return job.WriteConfiguration(configuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<IJournal> Journals(int id, int page, int itemsPerPage)
        {
            var job = Operation.JobService.Instance.Job(id);
            return job.ListJournals<Journal>(page, itemsPerPage);
        }

        /// <summary>
        /// Gets a collection of the Shield App Ids that are installed
        /// </summary>
        /// <returns>
        /// Collection of the Shield App ids
        /// </returns>
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
