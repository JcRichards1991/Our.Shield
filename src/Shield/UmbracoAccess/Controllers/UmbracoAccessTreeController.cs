using System;
using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Shield.UmbracoAccess.Controllers
{
    /// <summary>
    /// The Umbraco Access Tree Controller for the custom section
    /// </summary>
    [PluginController(Constants.App.Name)]
    [Tree(Constants.App.Alias, Constants.Tree.UmbracoAccess.Alias, Constants.Tree.UmbracoAccess.Title)]
    public class UmbracoAccessTreeController : TreeController
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

            if(id == Constants.RootNodeId)
            {
                treeNodeCollection.Add(this.CreateTreeNode(Constants.Tree.UmbracoAccess.NodeId, Constants.RootNodeId, queryStrings, Constants.Tree.UmbracoAccess.NodeName));
            }
            return treeNodeCollection;
        }
    }
}
