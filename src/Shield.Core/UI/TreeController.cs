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
            return null;
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
            var treeNodes = UI.TreeNode.Register;

            if(treeNodes != null && treeNodes.Any())
            {
                var tNodes = treeNodes.Select(x => UI.TreeNode.Create(x.Key)).OrderBy(x => x.Name);
                foreach(var treeNode in tNodes)
                {
                    if (id.Equals(treeNode.ParentId))
                    {
                        var qsList = queryStrings.ToList();
                        qsList.Add(new KeyValuePair<string, string>("configurationId", treeNode.ConfigurationId));

                        var qs = new FormDataCollection(qsList);

                        treeNodeCollection.Add(
                            this.CreateTreeNode(treeNode.Id,
                                treeNode.ParentId,
                                qs,
                                treeNode.Name,
                                treeNode.Icon,
                                treeNode.HasChildNodes,
                                treeNode.RoutePath));
                    }
                }
            }
            return treeNodeCollection;
        }
    }
}
