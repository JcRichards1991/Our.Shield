namespace Shield.Core.UI
{
    using System.Linq;
    using System.Net.Http.Formatting;
    using Models;
    using Umbraco.Web.Models.Trees;
    using Umbraco.Web.Mvc;

    /// <summary>
    /// The Umbraco Access Tree Controller for the custom section
    /// </summary>
    [PluginController(Constants.App.Name)]
    [Umbraco.Web.Trees.Tree(Constants.App.Alias, Constants.Tree.Alias, Constants.Tree.Title)]
    public class TreeController : Umbraco.Web.Trees.TreeController
    {
        /// <summary>
        /// Gets the menu for a node by it's Id.
        /// </summary>
        /// <param name="id">
        /// The Id of the node.
        /// </param>
        /// <param name="queryStrings">
        /// The query string parameters
        /// </param>
        /// <returns>
        /// Menu Item Collection containing the Menu Item(s).
        /// </returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if(id == Constants.Tree.RootNodeId || id == Constants.Tree.EnvironmentsRootId)
            {
                menu.Items.Add(new MenuItem("createEnvironment", "Create Environment"));
            }

            return menu;
        }

        /// <summary>
        /// Gets the Tree Node Collection.
        /// </summary>
        /// <param name="id">
        /// The Id.
        /// </param>
        /// <param name="queryStrings">
        /// the query string parameters.
        /// </param>
        /// <returns>
        /// Tree Node Collection containing the Tree Node(s).
        /// </returns>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var treeNodeCollection = new TreeNodeCollection();
            var environments = Operation.JobService.Instance.Environments;
            
            if (id == Constants.Tree.RootNodeId)
            {
                treeNodeCollection.Add(
                    CreateTreeNode(
                        Constants.Tree.EnvironmentsRootId,
                        Constants.Tree.RootNodeId,
                        queryStrings,
                        "Environments",
                        "icon-folder",
                        environments.Any()));

                return treeNodeCollection;
            }

            if (id == Constants.Tree.EnvironmentsRootId)
            {
                if(environments != null && environments.Any())
                {
                    foreach(var environment in environments)
                    {
                        var node = CreateTreeNode(
                            environment.Key.Id.ToString(),
                            Constants.Tree.EnvironmentsRootId,
                            queryStrings,
                            environment.Key.Name,
                            ((Environment) environment.Key).Icon,
                            environment.Value.Any(),
                            "Shield/Backoffice/Shield/Views/Environments.html?v=1.0.0");

                        //var publish = false;
                        //foreach (var job in environment.Value)
                        //{
                        //    if (job.ReadConfiguration().Enable)
                        //    {
                        //        publish = true;
                        //        break;
                        //    }
                        //}

                        //if (!publish)
                        //{
                        //    node.SetNotPublishedStyle();
                        //}

                        treeNodeCollection.Add(node);
                    }
                }
                return treeNodeCollection;
            }

            foreach (var environment in environments)
            {
                if (environment.Key.Id.ToString() == id)
                {
                    foreach (var job in environment.Value)
                    {
                        var app = App<IConfiguration>.Create(job.AppId);
                        var node = CreateTreeNode(
                            job.Id.ToString(),
                            environment.Key.Id.ToString(),
                            queryStrings,
                            app.Name,
                            app.Icon,
                            false,
                            "Shield/Backoffice/Shield/Views/App.html?v=1.0.0");

                        if (!job.ReadConfiguration().Enable)
                        {
                            node.SetNotPublishedStyle();
                        }
                        treeNodeCollection.Add(node);
                    }
                    return treeNodeCollection;
                }
            }
            return treeNodeCollection;
        }
    }
}
