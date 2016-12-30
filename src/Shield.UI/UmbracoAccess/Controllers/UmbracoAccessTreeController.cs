using System;
using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Shield.UI.UmbracoAccess.Controllers
{
    [PluginController(Constants.App.Name)]
    [Tree(Constants.App.Alias, Constants.Tree.UmbracoAccess.Alias, Constants.Tree.UmbracoAccess.Title)]
    public class ApplicationTreeController : TreeController
    {
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }

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
