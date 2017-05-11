namespace Shield.Core.Controllers
{
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
            var treeNodes = Models.TreeNode.Register;

            if(treeNodes != null && treeNodes.Any())
            {
                foreach(var treeNode in treeNodes)
                {
                    var tNode = Models.TreeNode.Create(treeNode.Key);

                    if (id.Equals(tNode.ParentId))
                    {
                        treeNodeCollection.Add(this.CreateTreeNode(tNode.Id, tNode.ParentId, queryStrings, tNode.Name, tNode.Icon, tNode.HasChildNodes, tNode.RoutePath));
                    }
                }
            }
            return treeNodeCollection;
        }
    }
}
