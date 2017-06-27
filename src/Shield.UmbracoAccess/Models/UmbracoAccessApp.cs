namespace Shield.UmbracoAccess.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using ClientDependency.Core;
    using Shield.Core.UI;
    using Shield.Core.Models;

    [AppEditor("/App_Plugins/Shield.UmbracoAccess/Views/UmbracoAccess.html?v=1.0.1")]
    [AppAsset(ClientDependencyType.Javascript, "/App_Plugins/Shield.UmbracoAccess/Scripts/UmbracoAccess.js?v=1.0.1")]
    [AppAsset(ClientDependencyType.Css, "/App_Plugins/Shield.UmbracoAccess/Css/UmbracoAccess.css?v=1.0.1")]
    public class UmbracoAccessApp : App<ViewModels.Configuration>
    {
        public override string Id => nameof(UmbracoAccess);

        public override string Name => "Umbraco Access";

        public override string Description => "Secure your backoffice access via IP restrictions";

        public override string Icon => "icon-stop-hand red";


        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new ViewModels.Configuration
                {
                    BackendAccessUrl = Core.ApplicationSettings.UmbracoPath,
                    IpAddresses = new IpAddress[0]
                };
            }
        }

        private static List<int> Ids = new List<int>();

        public override bool Execute(Core.Persistance.Data.Dto.Environment environment, Core.Persistance.Serialization.Configuration c)
        {
            var config = c as ViewModels.Configuration;

            Core.Operation.Fortress.UnwatchAll(Id);

            if (!config.Enable)
            {
                return true;
            }

            string url = string.Empty;
            switch (config.UnauthorisedUrlType)
            {
                case Enums.UnautorisedUrlType.Url:
                    url = config.UnauthorisedUrl;
                    break;

                case Enums.UnautorisedUrlType.XPath:
                    url = Core.Helpers.UrlHelper.GetUrl(config.UnauthorisedUrlXPath);
                    break;

                case Enums.UnautorisedUrlType.ContentPicker:
                    url = Core.Helpers.UrlHelper.GetUrl(Convert.ToInt32(config.UnauthorisedUrlContentPicker));
                    break;
            }

            var id = Core.Operation.Fortress.Watch(environment, this, new Regex(config.BackendAccessUrl), 2, (count, app) =>
            {
                var userIp = GetUserIp(app);
                if (!config.IpAddresses.Any(x => x.ipAddress == userIp))
                {
                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        app.Context.Response.Redirect(url, true);
                    }
                    else
                    {
                        app.Context.RewritePath(url);
                    }
                    return Core.Operation.Fortress.Cycle.Stop;
                }

                return Core.Operation.Fortress.Cycle.Continue;
            }, 0, null);

            Ids.Add(id);

            id = Core.Operation.Fortress.Watch(environment, this, null, 1, (count, app) => {
                return Core.Operation.Fortress.Cycle.Continue;

            }, 0, null);

            Ids.Add(id);

            return true;
        }

        private static string GetUserIp(HttpApplication app)
        {
            return string.Empty;
        }
    }
}
