namespace Shield.UmbracoAccess.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using Core.Models;
    using Core.UI;
    using Umbraco.Web;
    using System.Threading;
    using System.Net;
    using System.Web;
    using System.Web.Configuration;
    using System.Configuration;

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

        private static string allowKey = Guid.NewGuid().ToString();

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

                        if (ExecuteFirstTime(job, config))
                        {
                            return true;
                        }
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

            var ipv6s = new List<IPAddress>();

            foreach (var ip in config.IpAddresses)
            {
                if (ip.ipAddress.Equals("127.0.0.1"))
                    ip.ipAddress = "::1";

                IPAddress tempIp;
                if (!IPAddress.TryParse(ip.ipAddress, out tempIp))
                {
                    job.WriteJournal(new JournalMessage(""));
                    continue;
                }

                ipv6s.Add(tempIp.MapToIPv6());
            }

            var currentAppBackendUrl = Umbraco.Core.IO.IOHelper.ResolveUrl(config.BackendAccessUrl.EndsWith("/") ? config.BackendAccessUrl : config.BackendAccessUrl + "/");

            if (currentAppBackendUrl != ApplicationSettings.UmbracoPath)
            {
                job.WatchWebRequests(new Regex($"^{ ApplicationSettings.UmbracoPath.TrimEnd('/') }(/){{0,1}}$", RegexOptions.IgnoreCase), 10, (count, httpApp) =>
                {
                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        httpApp.Context.Response.Redirect(url, true);
                        return WatchCycle.Stop;
                    }
                    else
                    {
                        httpApp.Context.Items.Add(allowKey, false);
                        httpApp.Context.RewritePath(url);
                        return WatchCycle.Continue;
                    }
                });

                job.WatchWebRequests(new Regex($"^{ currentAppBackendUrl.TrimEnd('/') }(/){{0,1}}$", RegexOptions.IgnoreCase), 30, (count, httpApp) =>
                {
                    httpApp.Context.Items.Add(allowKey, true);
                    httpApp.Context.RewritePath(ApplicationSettings.UmbracoPath, false);

                    return WatchCycle.Continue;
                });
            }

            job.WatchWebRequests(new Regex(ApplicationSettings.UmbracoPath, RegexOptions.IgnoreCase), 30, (count, httpApp) =>
            {
                bool? doSecurity = (bool?)httpApp.Context.Items[allowKey];

                if (doSecurity == false)
                {
                    return WatchCycle.Continue;
                }

                var userIp = GetUserIp(httpApp);

                if (userIp == null || !ipv6s.Any(x => x.Equals(userIp)))
                {
                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        httpApp.Context.Response.Redirect(url, true);
                        return WatchCycle.Stop;
                    }
                    else
                    {
                        httpApp.Context.RewritePath(url);
                        return WatchCycle.Continue;
                    }
                }

                return WatchCycle.Continue;
            });


                return true;
        }

        private static ReaderWriterLockSlim webConfigLocker = new ReaderWriterLockSlim();

        private bool ExecuteFirstTime(IJob job, UmbracoAccessConfiguration config)
        {
            if (new Regex($"^{ config.BackendAccessUrl.Trim('~').TrimEnd('/') }(/){{0,1}}$").IsMatch(ApplicationSettings.UmbracoPath))
            {
                return false;
            }

            if (!webConfigLocker.TryEnterUpgradeableReadLock(1))
            {
                return true;
            }

            try
            {
                var path = HttpRuntime.AppDomainAppPath;
                if (!System.IO.Directory.Exists(path + ApplicationSettings.UmbracoPath.TrimEnd('/')))
                {
                    job.WriteJournal(new JournalMessage($"Unable to Rename and/or move directory from: { ApplicationSettings.UmbracoPath } to: { config.BackendAccessUrl }\nThe directory {ApplicationSettings.UmbracoPath} cannot be found"));
                    return false;
                }

                var webConfig = WebConfigurationManager.OpenWebConfiguration("~");
                var appSettings = (AppSettingsSection)webConfig.GetSection("appSettings");

                var umbracoPath = appSettings.Settings["umbracoPath"];
                var umbracoReservedPaths = appSettings.Settings["umbracoReservedPaths"];

                var regex = new Regex($"^({ umbracoPath.Value.Trim('~').TrimEnd('/') }(/){{0,1}})$");

                if (!regex.IsMatch(ApplicationSettings.UmbracoPath) && !regex.IsMatch(umbracoReservedPaths.Value))
                {
                    job.WriteJournal(new JournalMessage($"Unable to make neccessary changes to the web.config, appSetting keys: umbracoPath & umbracoReservedPaths dont contain the correct values"));
                    return false;
                }

                System.IO.Directory.Move(path + ApplicationSettings.UmbracoPath.Trim('/'), path + config.BackendAccessUrl.Trim('/'));

                umbracoReservedPaths.Value = umbracoReservedPaths.Value.Replace(umbracoPath.Value.TrimEnd('/'), config.BackendAccessUrl);
                umbracoPath.Value = $"~{config.BackendAccessUrl.TrimEnd('/')}";
                webConfig.Save();
            }
            catch(Exception ex)
            {
                job.WriteJournal(new JournalMessage($"Unexpected error occured, exception:\n{ex.Message}"));
                return false;
            }
            finally
            {
                webConfigLocker.ExitUpgradeableReadLock();
            }

            if (HttpContext.Current != null)
            {
                var response = HttpContext.Current.Response;
                response.Clear();
                response.Redirect(HttpContext.Current.Request.Url.ToString(), true);
            }

            HttpRuntime.UnloadAppDomain();

            return true;
        }

        private IPAddress GetUserIp(HttpApplication app)
        {
            var ip = app.Context.Request.UserHostAddress;

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
