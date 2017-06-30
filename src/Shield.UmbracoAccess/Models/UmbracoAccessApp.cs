namespace Shield.UmbracoAccess.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using ClientDependency.Core;
    using Shield.Core.Models;
    using Shield.Core.UI;
    using Umbraco.Web;

    [AppEditor("/App_Plugins/Shield.UmbracoAccess/Views/UmbracoAccess.html?v=1.0.1")]
    [AppAsset(ClientDependencyType.Javascript, "/App_Plugins/Shield.UmbracoAccess/Scripts/UmbracoAccess.js?v=1.0.1")]
    [AppAsset(ClientDependencyType.Css, "/App_Plugins/Shield.UmbracoAccess/Css/UmbracoAccess.css?v=1.0.1")]
    public class UmbracoAccessApp : App<UmbracoAccessConfiguration>
    {
        public override string Id => nameof(UmbracoAccess);

        public override string Name => "Umbraco Access";

        public override string Description => "Secure your backoffice access via IP restrictions";

        public override string Icon => "icon-stop-hand red";


        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new UmbracoAccessConfiguration
                {
                    BackendAccessUrl = ApplicationSettings.UmbracoPath,
                    IpAddresses = new IpAddress[0],
                    RedirectRewrite = Enums.RedirectRewrite.Redirect,
                    UnauthorisedUrlType = Enums.UnautorisedUrlType.Url
                };
            }
        }

        private static List<int> Ids = new List<int>();

        public override bool Execute(IJob job, IConfiguration c)
        {
            var config = c as UmbracoAccessConfiguration;

            job.UnwatchWebRequests();

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
                    url = UmbracoContext.Current.ContentCache.GetSingleByXPath(config.UnauthorisedUrlXPath).Url;
                    break;

                case Enums.UnautorisedUrlType.ContentPicker:
                    url = UmbracoContext.Current.ContentCache.GetById(Convert.ToInt32(config.UnauthorisedUrlContentPicker)).Url;
                    break;
            }

            job.WatchWebRequests(new Regex(config.BackendAccessUrl), 2, (count, httpApp) =>
            {
                var userIp = GetUserIp();
                if (!config.IpAddresses.Any(x => x.ipAddress == userIp))
                {
                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        httpApp.Context.Response.Redirect(url, true);
                    }
                    else
                    {
                        httpApp.Context.RewritePath(url);
                    }
                    return WatchCycle.Stop;
                }

                return WatchCycle.Continue;
            }, 0, null);

            return true;
        }

        private static string GetUserIp()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            else
            {
                ip = ip.Split(',')[0];
            }

            // Returns '::1' for when accessing from localhost
            // So convert to 127.0.0.1 for valid IP address
            if (ip.Equals("::1"))
            {
                ip = "127.0.0.1";
            }

            return ip;
        }
    }
}
