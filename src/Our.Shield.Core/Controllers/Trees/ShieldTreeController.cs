using System.Net.Http.Formatting;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using UmbConsts = Umbraco.Core.Constants;

namespace Our.Shield.Core.UI
{
    /// <summary>
    /// Shield Tree Controller to render the tree within the Settings Section
    /// </summary>
    [PluginController(Constants.App.Alias)]
    [Tree(
        UmbConsts.Applications.Settings,
        Constants.App.Alias,
        SortOrder = 100,
        TreeTitle = Constants.App.Name)]
    public class ShieldTreeController : TreeController
    {
        /// <inheritdoc />
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);

            root.RoutePath = $"{UmbConsts.Applications.Settings}/{Constants.App.Alias}/Dashboard";
            root.Icon = Constants.App.Icon;
            root.HasChildren = false;

            return root;
        }

        /// <inheritdoc />
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == UmbConsts.System.Root.ToString())
            {
                menu.Items.Add<ActionNew>(Services.TextService, true);
                menu.Items.Add<ActionSort>(Services.TextService, true);

                return menu;
            }

            return null;
        }

        /// <inheritdoc />
        protected override TreeNodeCollection GetTreeNodes(string idStr, FormDataCollection queryStrings)
        {
            return new TreeNodeCollection();
        }
    }
}
