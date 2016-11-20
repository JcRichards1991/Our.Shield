using System;
using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Shield.UI.Shield
{
    [PluginController(Constants.App.Name)]
    [Tree(Constants.App.Alias, Constants.Tree.Alias, Constants.Tree.Title)]
    public class ApplicationTreeController : TreeController
    {
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;
        }

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var treeNodeCollection = new TreeNodeCollection();

            if(id == Constants.NodeId.RootId)
            {
                treeNodeCollection.Add(this.CreateTreeNode("-3131", Constants.NodeId.RootId, queryStrings, "Umbraco Access"));
            }

            return treeNodeCollection;
        }
    }
}
