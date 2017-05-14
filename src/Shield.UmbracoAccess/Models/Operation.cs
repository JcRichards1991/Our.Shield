namespace Shield.UmbracoAccess.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    public class Operation : Core.Models.Operation<ViewModels.Configuration>
    {
        public override string Id => nameof(UmbracoAccess);


        public override Core.Models.Configuration DefaultConfiguration
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

        public override bool Execute(Core.Models.Configuration c)
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

            var id = Core.Operation.Fortress.Watch(Id, new Regex(config.BackendAccessUrl), 2, (count, app) =>
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

            id = Core.Operation.Fortress.Watch(Id, null, 1, (count, app) => {
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
