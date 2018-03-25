using Our.Shield.Core.Services;
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
        /// <param name="idText">The Id of the node</param>
        /// <param name="queryStrings">The query string parameters</param>
        /// <returns>Menu Item Collection containing the Menu Item(s)</returns>
        protected override MenuItemCollection GetMenuForNode(string idText, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();
            var id = int.Parse(idText);

            switch (id)
            {
                case global::Umbraco.Core.Constants.System.Root:
                    menu.Items.Add<ActionNew>("Create Environment");
                    menu.Items.Add<ActionRefresh>("Reload Environments");
                    menu.Items.Add<ActionSort>("Sort Environments");
                    return menu;

                case Constants.Tree.DefaultEnvironmentId:
                    menu.Items.Add<ActionRefresh>("Reload Apps");
                    return menu;

                default:
                    var environments = JobService.Instance.Environments;
                    if (!environments.Any(x => x.Key.Id.Equals(id)))
                        return menu;

                    menu.Items.Add<ActionDelete>("Delete Environment");
                    menu.Items.Add<ActionRefresh>("Reload Apps");
                    return menu;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the Tree Node Collection.
        /// </summary>
        /// <param name="idText">The Id</param>
        /// <param name="queryStrings">The query string parameters</param>
        /// <returns>Tree Node Collection containing the Tree Node(s)</returns>
        protected override TreeNodeCollection GetTreeNodes(string idText, FormDataCollection queryStrings)
        {
            var id = int.Parse(idText);
            var treeNodeCollection = new TreeNodeCollection();
            var environments = JobService.Instance.Environments.OrderBy(x => x.Key.SortOrder).ToList();
            
            if (id == global::Umbraco.Core.Constants.System.Root)
            {
                if (!environments.Any())
                    return treeNodeCollection;

                foreach (var environment in environments)
                {
                    var environmentId = environment.Key.Id.ToString();
                    var node = CreateTreeNode(
                        environmentId,
                        global::Umbraco.Core.Constants.System.Root.ToString(),
                        queryStrings,
                        environment.Key.Name,
                        ((Models.Environment)environment.Key).Icon,
                        environment.Value.Any());

                    if (!environment.Key.Enable)
                    {
                        node.SetNotPublishedStyle();
                    }

                    treeNodeCollection.Add(node);
                }
                return treeNodeCollection;
            }

            foreach (var environment in environments)
            {
                if (environment.Key.Id != id)
                    continue;

                foreach (var job in environment.Value.OrderBy(x => x.App.Name))
                {
                    var jobId = job.Id.ToString();
                    var node = CreateTreeNode(
                        jobId,
                        environment.Key.Id.ToString(),
                        queryStrings,
                        job.App.Name,
                        job.App.Icon,
                        false);

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
