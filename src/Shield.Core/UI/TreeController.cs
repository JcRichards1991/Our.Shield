namespace Shield.Core.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Formatting;
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
            
            if (id == Constants.Tree.RootNodeId)
            {
                treeNodeCollection.Add(
                    this.CreateTreeNode(
                        Constants.Tree.EnvironmentsRootId,
                        Constants.Tree.RootNodeId,
                        queryStrings,
                        "Environments",
                        "icon-folder",
                        true));

                return treeNodeCollection;
            }

            if (id == Constants.Tree.EnvironmentsRootId)
            {
                var environments = Operation.JobService.Instance.Environments;

                if(environments != null && environments.Any())
                {
                    foreach(var environment in environments)
                    {
                        treeNodeCollection.Add(this.CreateTreeNode(
                            environment.Key.Id.ToString(),
                            Constants.Tree.EnvironmentsRootId,
                            queryStrings,
                            environment.Key.Name));
                    }
                }
                return treeNodeCollection;
            }
            
            var treeNodes = Operation.App<Persistance.Serialization.Configuration>.Register;
            if (treeNodes != null && treeNodes.Any())
            {
                var tNodes = treeNodes.Select(x => Operation.App<Persistance.Serialization.Configuration>.Create(x.Key)).OrderBy(x => x.Name);

                var index = 0;

                foreach (var treeNode in tNodes)
                {
                    treeNodeCollection.Add(
                        this.CreateTreeNode(index.ToString(),
                            Constants.Tree.RootNodeId,
                            queryStrings,
                            treeNode.Name,
                            treeNode.Icon));

                    index++;
                }
            }
            return treeNodeCollection;
        }
    }
}
