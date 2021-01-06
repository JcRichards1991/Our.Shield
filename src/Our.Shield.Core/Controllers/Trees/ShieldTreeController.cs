using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using UmbConsts = Umbraco.Core.Constants;

namespace Our.Shield.Core.UI
{
    /// <summary>
    /// Shield Tree Conteoller to render the tree within the Settings Section
    /// </summary>
    [PluginController(Constants.App.Name)]
    [Tree(
        UmbConsts.Applications.Settings,
        Constants.App.Alias,
        SortOrder = 100,
        TreeGroup = "",
        TreeTitle = Constants.App.Name)]
    public class ShieldTreeController : TreeController
    {
        /// <inheritdoc />
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.RoutePath = $"{UmbConsts.Applications.Settings}/{Constants.App.Alias}/Dashboard";
            root.Icon = Constants.App.Icon;
            root.HasChildren = true;

            return root;
        }

        /// <inheritdoc />
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            return null;

            //var menu = new MenuItemCollection();

            //if (id == UmbConsts.System.Root.ToString())
            //{
            //    menu.Items.Add<ActionNew>("Create Environment");
            //    menu.Items.Add<ActionRefresh>("Reload Environments");
            //    menu.Items.Add<ActionSort>("Sort Environments");

            //    return menu;
            //}

            //if (!Guid.TryParse(id, out var key))
            //{
            //    return menu;
            //}

            //var environments = JobService.Instance.Environments;

            //if (environments
            //        .FirstOrDefault(x => x.Key.Key == key && x.Key.Id == Constants.Tree.DefaultEnvironmentId)
            //        .Key != null)
            //{
            //    menu.Items.Add<ActionRefresh>("Reload Apps");
            //    return menu;
            //}

            //if (environments.All(x => x.Key.Key != key))
            //    return menu;

            //menu.Items.Add<ActionDelete>("Delete Environment");
            //menu.Items.Add<ActionRefresh>("Reload Apps");
            //return menu;
        }

        /// <inheritdoc />
        protected override TreeNodeCollection GetTreeNodes(string idStr, FormDataCollection queryStrings)
        {
            return new TreeNodeCollection();

            //var treeNodeCollection = new TreeNodeCollection();
            //var environments = JobService.Instance.Environments.OrderBy(x => x.Key.SortOrder).ToList();

            //if (idStr == UmbConsts.System.Root.ToString())
            //{
            //    if (!environments.Any())
            //    {
            //        return treeNodeCollection;
            //    }

            //    foreach (var environment in environments)
            //    {
            //        var id = environment.Key.Key.ToString();
            //        var node = CreateTreeNode(
            //            id,
            //            global::Umbraco.Core.Constants.System.Root.ToString(),
            //            queryStrings,
            //            environment.Key.Name,
            //            ((Models.Environment)environment.Key).Icon,
            //            environment.Value.Any(),
            //            $"shield/shield/Environment/{id}");

            //        if (!environment.Key.Enable)
            //        {
            //            node.SetNotPublishedStyle();
            //        }

            //        treeNodeCollection.Add(node);
            //    }

            //    return treeNodeCollection;
            //}

            //if (!Guid.TryParse(idStr, out var envKey))
            //{
            //    return treeNodeCollection;
            //}

            //foreach (var environment in environments)
            //{
            //    if (environment.Key.Key != envKey)
            //    {
            //        continue;
            //    }

            //    foreach (var job in environment.Value.OrderBy(x => x.App.Name))
            //    {
            //        var id = job.Key.ToString();
            //        var node = CreateTreeNode(
            //            id,
            //            environment.Key.Id.ToString(),
            //            queryStrings,
            //            job.App.Name,
            //            job.App.Icon,
            //            $"shield/shield/App/{id}");

            //        if (!job.ReadConfiguration().Enable || !environment.Key.Enable)
            //        {
            //            node.SetNotPublishedStyle();
            //        }

            //        treeNodeCollection.Add(node);
            //    }

            //    return treeNodeCollection;
            //}

            //return treeNodeCollection;
        }
    }
}
