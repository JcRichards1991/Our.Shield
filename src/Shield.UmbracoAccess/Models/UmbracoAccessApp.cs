namespace Shield.UmbracoAccess.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using ClientDependency.Core;
    using Core.Models;
    using Core.UI;
    using Umbraco.Web;
    using System.Threading;
    using System.Net;

    [AppEditor("/App_Plugins/Shield.UmbracoAccess/Views/UmbracoAccess.html?v=1.0.1")]
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

        private static ReaderWriterLockSlim environemntIdsLocker = new ReaderWriterLockSlim();
        private static List<int> environmentIds = new List<int>();

        public override bool Execute(IJob job, IConfiguration c)
        {
            var config = c as UmbracoAccessConfiguration;

            if (environemntIdsLocker.TryEnterUpgradeableReadLock(1))
            {
                try
                {
                    if (!environmentIds.Contains(job.Environment.Id))
                    {
                        if (environemntIdsLocker.TryEnterWriteLock(1))
                        {
                            try
                            {
                                environmentIds.Add(job.Environment.Id);
                            }
                            finally
                            {
                                environemntIdsLocker.ExitWriteLock();
                            }
                        }
                        else
                        {
                            return false;
                        }

                        //ExecuteFirstTime(job, config);
                        //return true;
                    }
                }
                finally
                {
                    environemntIdsLocker.ExitUpgradeableReadLock();
                }
            }
            else
            {
                return false;
            }


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

            if (string.IsNullOrEmpty(url))
            {
                job.WriteJournal(new JournalMessage(""));
                return true;
            }
            else
            {
                var ipv6s = new List<IPAddress>();

                foreach (var ip in config.IpAddresses)
                {
                    IPAddress tempIp;
                    if (!IPAddress.TryParse(ip.ipAddress, out tempIp))
                    {
                        job.WriteJournal(new JournalMessage(""));
                        continue;
                    }

                    if (tempIp.ToString().Equals("127.0.0.1"))
                        tempIp = IPAddress.Parse("::1");

                    ipv6s.Add(tempIp.MapToIPv6());
                }

                var currentAppBackendUrl = config.BackendAccessUrl.EndsWith("/") ? config.BackendAccessUrl : config.BackendAccessUrl + "/";

                if (config.BackendAccessUrl != ApplicationSettings.UmbracoPath)
                {
                    job.WatchWebRequests(new Regex(ApplicationSettings.UmbracoPath), 1, (count, httpApp) =>
                    {
                        var localPath = httpApp.Context.Request.UrlReferrer == null
                            ? string.Empty
                            : (httpApp.Context.Request.UrlReferrer.LocalPath + (httpApp.Context.Request.UrlReferrer.LocalPath.EndsWith("/") ? string.Empty : "/"));

                        if (httpApp.Context.Request.UrlReferrer == null
                            || httpApp.Context.Request.UrlReferrer.Host != httpApp.Context.Request.Url.Host
                            || !(httpApp.Context.Request.UrlReferrer.Host == httpApp.Context.Request.Url.Host
                                && localPath.Equals(currentAppBackendUrl)))
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
                }

                job.WatchWebRequests(new Regex("^" + config.BackendAccessUrl + "(/){0,1}$"), 10, (count, httpApp) =>
                {
                    var userIp = GetUserIp(httpApp);

                    if (userIp == null || !ipv6s.Any(x => x.Equals(userIp)))
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

                    if (!httpApp.Context.Request.Url.AbsolutePath.EndsWith("/"))
                    {
                        httpApp.Context.Response.Redirect(currentAppBackendUrl, false);
                        return WatchCycle.Stop;
                    }

                    if (config.BackendAccessUrl != ApplicationSettings.UmbracoPath)
                    {
                        httpApp.Context.RewritePath(ApplicationSettings.UmbracoPath);
                    }

                    return WatchCycle.Continue;
                }, 0, null);
            }

            return true;
        }

        private void ExecuteFirstTime(IJob job, UmbracoAccessConfiguration config)
        {
            // TODO: if config is different, hard save
            // Otherwise leave

            if (HttpContext.Current != null)
            {
                var response = HttpContext.Current.Response;
                response.Clear();
                response.Redirect(HttpContext.Current.Request.Url.ToString(), true);
            }

            HttpRuntime.UnloadAppDomain();
        }

        private IPAddress GetUserIp(HttpApplication app)
        {
            var ip = app.Context.Request.ServerVariables["REMOTE_ADDR"];

            if (ip.Equals("127.0.0.1"))
                ip = "::1";

            IPAddress tempIp;

            if(IPAddress.TryParse(ip, out tempIp))
            {
                return tempIp.MapToIPv6();
            }

            return null;
        }
    }
}
