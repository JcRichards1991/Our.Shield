using Our.Shield.Core.Services;
using Our.Shield.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
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
        private readonly IEnvironmentService _environmentService;
        private readonly IAppService _appService;

        /// <summary>
        /// Initializes a new instance of <see cref="ShieldTreeController"/> class.
        /// </summary>
        /// <param name="globalSettings"><see cref="IGlobalSettings"/></param>
        /// <param name="umbContextAccessor"><see cref="IUmbracoContextAccessor"/></param>
        /// <param name="sqlContext"><see cref="ISqlContext"/></param>
        /// <param name="serviceContext"><see cref="ServiceContext"/></param>
        /// <param name="appCaches"><see cref="AppCaches"/></param>
        /// <param name="profilingLogger"><see cref="IProfilingLogger"/></param>
        /// <param name="runtimeState"><see cref="IRuntimeState"/></param>
        /// <param name="umbHelper"><see cref="UmbracoHelper"/></param>
        /// <param name="environmentService"><see cref="IEnvironmentService"/></param>
        /// <param name="appService"><see cref="IAppService"/></param>
        public ShieldTreeController(
            IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbContextAccessor,
            ISqlContext sqlContext,
            ServiceContext serviceContext,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IRuntimeState runtimeState,
            UmbracoHelper umbHelper,
            IEnvironmentService environmentService,
            IAppService appService)
            : base (globalSettings, umbContextAccessor, sqlContext, serviceContext, appCaches, profilingLogger, runtimeState, umbHelper)
        {
            GuardClauses.NotNull(environmentService, nameof(environmentService));
            GuardClauses.NotNull(appService, nameof(appService));

            _environmentService = environmentService;
            _appService = appService;
        }

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
            var menu = new MenuItemCollection();

            if (id == UmbConsts.System.Root.ToString())
            {
                menu.Items.Add<ActionNew>(Services.TextService, true);
                menu.Items.Add<ActionSort>(Services.TextService, true);
                menu.Items.Add(new RefreshNode(Services.TextService, true));

                return menu;
            }
            else if (Guid.TryParse(id, out var key))
            {
                var response = _environmentService.Get(key).Result;

                if (response.Environment != null)
                {
                    menu.Items.Add<ActionDelete>(Services.TextService, true);

                    return menu;
                }
            }

            return null;
        }

        /// <inheritdoc />
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var tree = new TreeNodeCollection();

            if (id == UmbConsts.System.Root.ToString())
            {
                var response = _environmentService.Get().Result;

                foreach (var environment in response.Environments)
                {
                    tree.Add(
                        CreateTreeNode(
                            environment.Key.ToString(),
                            UmbConsts.System.Root.ToString(),
                            new FormDataCollection(Enumerable.Empty<KeyValuePair<string, string>>()),
                            environment.Name,
                            environment.Icon,
                            true,
                            $"settings/{Constants.App.Alias}/environment/{environment.Key}"));
                }
            }
            else if (Guid.TryParse(id, out var key))
            {
                var result = _environmentService.Get(key).Result;

                if (result.Environment != null)
                {
                    var appsResult = _appService.GetApps(result.Environment.Key).Result;

                    foreach(var app in appsResult.Apps)
                    {
                        tree.Add(
                            CreateTreeNode(
                                app.Key.Key.ToString(),
                                id,
                                new FormDataCollection(Enumerable.Empty<KeyValuePair<string, string>>()),
                                Services.TextService.Localize($"Shield.{app.Key.Id}/Name"),
                                app.Key.Icon,
                                false,
                                $"settings/{Constants.App.Alias}/app/{app.Key.Key}"));
                    }
                }
            }

            return tree;
        }
    }
}
