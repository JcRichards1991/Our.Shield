namespace Shield.Core.UI
{
    using System.Linq;
    using System.Web.Http;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;
    using Newtonsoft.Json.Linq;
    using Shield.Core.Models;
    using System.Collections.Generic;
    using System.Reflection;
    using Newtonsoft.Json;

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

            foreach(var environment in environments)
            {
                if (id == environment.Key.Id)
                {
                    return new TreeView
                    {
                        Type = UI.TreeView.TreeViewType.Environment,
                        Name = environment.Key.Name,
                        Description = $"Configure and view events for the {environment.Key.Name} environment",
                        Environments = environments.Keys,
                        Environment = environment.Key,
                        Apps = environment.Value.Select(x => App<IConfiguration>.Create(x.AppId))
                    };
                }
                foreach (var job in environment.Value)
                {
                    if (id == job.Id)
                    {
                        var app = App<IConfiguration>.Create(job.AppId);
                        var appAssests = new AppAssest
                        {
                            View = app.GetType().GetCustomAttribute<AppEditorAttribute>()?.FilePath ?? null,

                            Stylesheets = app.GetType().GetCustomAttributes<AppAssetAttribute>()
                                .Where(x => x.AssetType == ClientDependency.Core.ClientDependencyType.Css)
                                .Select(x => x.FilePath),

                            Scripts = app.GetType().GetCustomAttributes<AppAssetAttribute>()
                                .Where(x => x.AssetType == ClientDependency.Core.ClientDependencyType.Javascript)
                                .Select(x => x.FilePath)
                        };
                        
                        return new TreeView
                        {
                            Type = UI.TreeView.TreeViewType.App,
                            Name = app.Name,
                            Description = app.Description,
                            Environments = environments.Keys,
                            Environment = environment.Key,
                            App = app,
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

        [HttpGet]
        public IEnumerable<IJournal> Journals(int id, int page, int itemsPerPage)
        {
            var job = Operation.JobService.Instance.Job(id);
            return job.ListJournals<Journal>(page, itemsPerPage);
        }
    }
}
