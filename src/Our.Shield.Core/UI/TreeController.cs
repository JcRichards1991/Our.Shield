using Our.Shield.Core.Services;
using System;
using System.Linq;
using System.Net.Http.Formatting;
using umbraco.BusinessLogic.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Our.Shield.Core.UI
{
    /// <inheritdoc />
    /// <summary>
    /// The Umbraco Access Tree Controller for the custom section
    /// </summary>
    [PluginController(Constants.App.Name)]
    [Tree(Constants.App.Alias, Constants.App.Alias, Constants.App.Name)]
    public class TreeController : Umbraco.Web.Trees.TreeController
    {
        /// <inheritdoc />
        /// <summary>
        /// Gets the menu for a node by it's Id
        /// </summary>
        /// <param name="idStr">The Id of the node</param>
        /// <param name="queryStrings">The query string parameters</param>
        /// <returns>Menu Item Collection containing the Menu Item(s)</returns>
        protected override MenuItemCollection GetMenuForNode(string idStr, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (idStr == global::Umbraco.Core.Constants.System.Root.ToString())
            {
                menu.Items.Add<ActionNew>("Create Environment");
                menu.Items.Add<ActionRefresh>("Reload Environments");
                menu.Items.Add<ActionSort>("Sort Environments");
                return menu;
            }

            if (!Guid.TryParse(idStr, out var key))
                return menu;

            var environments = JobService.Instance.Environments;

            if (environments
                    .FirstOrDefault(x => x.Key.Key == key && x.Key.Id == Constants.Tree.DefaultEnvironmentId)
                    .Key != null)
            {
                menu.Items.Add<ActionRefresh>("Reload Apps");
                return menu;
            }

            if (environments.All(x => x.Key.Key != key))
            return menu;

            menu.Items.Add<ActionDelete>("Delete Environment");
            menu.Items.Add<ActionRefresh>("Reload Apps");
            return menu;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the Tree Node Collection.
        /// </summary>
        /// <param name="idStr">The Id</param>
        /// <param name="queryStrings">The query string parameters</param>
        /// <returns>Tree Node Collection containing the Tree Node(s)</returns>
        protected override TreeNodeCollection GetTreeNodes(string idStr, FormDataCollection queryStrings)
        {
            var treeNodeCollection = new TreeNodeCollection();
            var environments = JobService.Instance.Environments.OrderBy(x => x.Key.SortOrder).ToList();

            if (idStr == global::Umbraco.Core.Constants.System.Root.ToString())
            {
                if (!environments.Any())
                    return treeNodeCollection;

                foreach (var environment in environments)
                {
                    var id = environment.Key.Key.ToString();
                    var node = CreateTreeNode(
                        id,
                        global::Umbraco.Core.Constants.System.Root.ToString(),
                        queryStrings,
                        environment.Key.Name,
                        ((Models.Environment)environment.Key).Icon,
                        environment.Value.Any(),
                        $"shield/shield/Environment/{id}");

                    if (!environment.Key.Enable)
                    {
                        node.SetNotPublishedStyle();
                    }
                    treeNodeCollection.Add(node);
                }
                return treeNodeCollection;
            }

            if (!Guid.TryParse(idStr, out var envKey))
                return treeNodeCollection;

            foreach (var environment in environments)
            {
                if (environment.Key.Key != envKey)
                    continue;

                foreach (var job in environment.Value.OrderBy(x => x.App.Name))
                {
                    var id = job.Key.ToString();
                    var node = CreateTreeNode(
                        id,
                        environment.Key.Id.ToString(),
                        queryStrings,
                        job.App.Name,
                        job.App.Icon,
                        $"shield/shield/App/{id}");

                    if (!job.ReadConfiguration().Enable || !environment.Key.Enable)
                    {
                        node.SetNotPublishedStyle();
                    }
                    treeNodeCollection.Add(node);
                }
                return treeNodeCollection;
            }
            return treeNodeCollection;
        }
    }
}
