namespace Our.Shield.BackofficeAccess.Models
{
    using Core.Models;
    using Core.UI;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using Umbraco.Core;
    using Umbraco.Core.Configuration;
    using Umbraco.Web;
    using Umbraco.Web.Routing;

    /// <summary>
    /// 
    /// </summary>
    [AppEditor("/App_Plugins/Shield.BackofficeAccess/Views/BackofficeAccess.html?version=1.0.0-pre-alpha")]
    public class BackofficeAccessApp : App<BackofficeAccessConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(BackofficeAccess);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Backoffice Access";

        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Change the backoffice access URL and/or secure your backoffice URL via IP restrictions";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-stop-hand red";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new BackofficeAccessConfiguration
                {
                    BackendAccessUrl = ApplicationSettings.UmbracoPath,
                    IpAddresses = new IpAddress[0],
                    RedirectRewrite = Enums.RedirectRewrite.Redirect,
                    UnauthorisedUrlType = Enums.UnautorisedUrlType.Url
                };
            }
        }

        private int firstExecute = 0;

        private readonly string allowKey = Guid.NewGuid().ToString();
        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);        //   Once per day

        private string UnauthorisedUrl(IJob job, BackofficeAccessConfiguration config)
        {
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(allowKey, () => {
                string url = null;
                var journalMessage = new JournalMessage();
                var umbContext = UmbracoContext.Current;

                switch (config.UnauthorisedUrlType)
                {
                    case Enums.UnautorisedUrlType.Url:
                        if (!string.IsNullOrEmpty(config.UnauthorisedUrl))
                        {
                            url = config.UnauthorisedUrl;
                        }
                        else
                        {
                            journalMessage.Message = "Error: No Unauthorized URL set in configuration";
                        }
                        break;

                    case Enums.UnautorisedUrlType.XPath:
                        var xpathNode = umbContext.ContentCache.GetSingleByXPath(config.UnauthorisedUrlXPath);

                        if(xpathNode != null)
                        {
                            url = xpathNode.Url;
                        }
                        else
                        {
                            journalMessage.Message = "Error: Unable to get the unauthorized URL from the specified XPath expression.";
                        }
                        break;

                    case Enums.UnautorisedUrlType.ContentPicker:
                        int id;

                        if(int.TryParse(config.UnauthorisedUrlContentPicker, out id))
                        {
                            var contentPickerNode = umbContext.ContentCache.GetById(id);

                            if (contentPickerNode != null)
                            {
                                url = contentPickerNode.Url;
                            }
                            else
                            {
                                journalMessage.Message = "Error: Unable to get the unauthorized URL from the selected unauthorized URL content picker. Please ensure the selected page is published and not deleted.";
                            }
                        }
                        else
                        {
                            journalMessage.Message = "Error: Unable to parse the selected unauthorized URL content picker id to integer. Please ensure a valid content node is selected.";
                        }
                        break;

                    default:
                        journalMessage.Message = "Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected.";
                        break;
                }

                if (url == null)
                {
                    if(!string.IsNullOrEmpty(journalMessage.Message))
                    {
                        job.WriteJournal(journalMessage);
                    }

                    return null;
                }

                return url;
            }, CacheLength) as string;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            var config = c as BackofficeAccessConfiguration;

            if (Interlocked.CompareExchange(ref firstExecute, 1, 0) == 0)
            {
                //TODO: Un-comment out code for release when method is more 'stable'

                //if (ExecuteFirstTime(job, config))
                //{
                //    return true;
                //}
            }

            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(allowKey);
            job.UnwatchWebRequests();

            if (!config.Enable)
            {
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
                job.WatchWebRequests(new Regex("^((" + currentAppBackendUrl.TrimEnd('/') + "(/){0,1})|(" + currentAppBackendUrl + ".*\\.([A-Za-z0-9]){3,5}))$", RegexOptions.IgnoreCase), 10, (count, httpApp) =>
                {
                    var rewritePath = (httpApp.Request.Url.AbsolutePath.Length > currentAppBackendUrl.Length) ?
                        ApplicationSettings.UmbracoPath +
                        httpApp.Request.Url.AbsolutePath.Substring(currentAppBackendUrl.Length) :
                        ApplicationSettings.UmbracoPath;

                    httpApp.Context.Items.Add(allowKey, true);
                    httpApp.Context.RewritePath(rewritePath);

                    return WatchCycle.Restart;
                });

                job.WatchWebRequests(new Regex($"^{ ApplicationSettings.UmbracoPath.TrimEnd('/') }(/){{0,1}}$", RegexOptions.IgnoreCase), 20, (count, httpApp) =>
                {
                    if ((bool?)httpApp.Context.Items[allowKey] == true)
                    {
                        httpApp.Context.Items[allowKey] = false;
                        return WatchCycle.Continue;
                    }

                    var url = UnauthorisedUrl(job, config);
                    if(url == null)
                    {
                        return WatchCycle.Continue;
                    }

                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        httpApp.Context.Response.Redirect(url, true);
                        return WatchCycle.Stop;
                    }

                    httpApp.Context.RewritePath(url);
                    return WatchCycle.Restart;
                });
            }

            job.WatchWebRequests(new Regex($"^{ ApplicationSettings.UmbracoPath.TrimEnd('/') }(/){{0,1}}$", RegexOptions.IgnoreCase), 1000, (count, httpApp) =>
            {
                var userIp = GetUserIp(httpApp);

                if (userIp == null || !ipv6s.Any(x => x.Equals(userIp)))
                {
                    var url = UnauthorisedUrl(job, config);

                    if(url == null)
                    {
                        return WatchCycle.Continue;
                    }

                    if (config.RedirectRewrite == Enums.RedirectRewrite.Redirect)
                    {
                        httpApp.Context.Response.Redirect(url, true);
                        return WatchCycle.Stop;
                    }
                    else
                    {
                        httpApp.Context.RewritePath(url);
                        return WatchCycle.Restart;
                    }
                }

                return WatchCycle.Continue;
            });

            return true;
        }

        private ReaderWriterLockSlim webConfigLocker = new ReaderWriterLockSlim();

        private bool ExecuteFirstTime(IJob job, BackofficeAccessConfiguration config)
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
                if (!Directory.Exists(path + ApplicationSettings.UmbracoPath.TrimEnd('/')))
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
                    job.WriteJournal(new JournalMessage($"Unable to make necessary changes to the web.config, appSetting keys: umbracoPath & umbracoReservedPaths doesn't contain the expected values"));
                    return false;
                }

                Directory.Move(path + ApplicationSettings.UmbracoPath.Trim('/'), path + config.BackendAccessUrl.Trim('/'));

                umbracoReservedPaths.Value = umbracoReservedPaths.Value.Replace(umbracoPath.Value.TrimEnd('/'), config.BackendAccessUrl);
                umbracoPath.Value = $"~{config.BackendAccessUrl.TrimEnd('/')}";
                webConfig.Save();
            }
            catch(Exception ex)
            {
                job.WriteJournal(new JournalMessage($"Unexpected error occurred, exception:\n{ex.Message}"));
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
