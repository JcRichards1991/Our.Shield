using Our.Shield.Core;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Helpers;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;

namespace Our.Shield.FrontendAccess.Models
{
    [AppEditor("/App_Plugins/Shield.FrontendAccess/Views/FrontendAccess.html?version=1.0.3")]
    public class FrontendAccessApp : App<FrontendAccessConfiguration>
    {
        /// <summary>
        /// 
        /// </summary>
        public override string Description => "Lock down the frontend to only be viewed by Umbraco Authenticated Users and/or secure the frontend via IP restrictions";

        /// <summary>
        /// 
        /// </summary>
        public override string Icon => "icon-combination-lock red";

        /// <summary>
        /// 
        /// </summary>
        public override string Id => nameof(FrontendAccess);

        /// <summary>
        /// 
        /// </summary>
        public override string Name => "Frontend Access";

        /// <summary>
        /// 
        /// </summary>
        public override IConfiguration DefaultConfiguration
        {
            get
            {
                return new FrontendAccessConfiguration
                {
                    UmbracoUserEnable = true,
                    IpAddressesAccess = Enums.IpAddressesAccess.Unrestricted,
                    IpEntries = new IpEntry[0],
                    UnauthorisedAction = Enums.UnauthorisedAction.Redirect,
                    UrlType = new UrlType
                    {
                        UrlSelector = Enums.UrlType.Url
                    }
                };
            }
        }

        private readonly string AllowKey = Guid.NewGuid().ToString();

        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay);

        private bool IsRequestAllowed(HttpApplication httpApp, string url)
        {
            if (httpApp.Context.Request.Url.AbsolutePath.Equals(url) || (bool?)httpApp.Context.Items[AllowKey] == true)
            {
                return true;
            }

            return false;
        }

        private WatchCycle DenyAccess(IJob job, HttpApplication httpApp, string unauthorisedUrl, Enums.UnauthorisedAction action, IPAddress userIp = null)
        {
            if (userIp == null)
            {
                userIp = AccessHelper.ConvertToIpv6(httpApp.Context.Request.UserHostAddress);
            }

            job.WriteJournal(new JournalMessage($"Unauthenticated user tried to access page: {httpApp.Context.Request.Url.AbsolutePath}; From IP Address: {userIp}; Access was denied"));

            httpApp.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            if (unauthorisedUrl == null)
            {
                return WatchCycle.Stop;
            }

            if (action == Enums.UnauthorisedAction.Rewrite)
            {
                httpApp.Context.Server.TransferRequest(unauthorisedUrl + httpApp.Request.Url.Query, true);
                return WatchCycle.Stop;
            }

            httpApp.Context.Response.Redirect(unauthorisedUrl, true);
            return WatchCycle.Stop;
        }

        private WatchCycle AllowAccess(HttpApplication httpApp)
        {
            httpApp.Context.Items.Add(AllowKey, true);
            return WatchCycle.Continue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public override bool Execute(IJob job, IConfiguration c)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(AllowKey);
            job.UnwatchWebRequests();

            if (!c.Enable || !job.Environment.Enable)
            {
                return true;
            }

            var config = c as FrontendAccessConfiguration;
            var hardUmbracoLocation = ApplicationSettings.UmbracoPath;
            var regex = new Regex("^/$|^(/(?!" + hardUmbracoLocation.Trim('/') + ")[\\w-/_]+?)$", RegexOptions.IgnoreCase);

            var whiteList = new List<IPAddress>();

            if (config.IpAddressesAccess == Enums.IpAddressesAccess.Restricted)
            {
                foreach (var ipEntry in config.IpEntries)
                {
                    var ip = AccessHelper.ConvertToIpv6(ipEntry.IpAddress);
                    if (ip == null)
                    {
                        job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {ipEntry.IpAddress}, unable to add to white-list"));
                        continue;
                    }

                    whiteList.Add(ip);
                }
            }

            if (config.UmbracoUserEnable && config.IpAddressesAccess == Enums.IpAddressesAccess.Unrestricted)
            {
                job.WatchWebRequests(regex, 75, (count, httpApp) =>
                {
                    var unauthorisedUrl = AccessHelper.UnauthorisedUrl(AllowKey, CacheLength, job, config.UrlType);

                    if (IsRequestAllowed(httpApp, unauthorisedUrl))
                    {
                        return WatchCycle.Continue;
                    }

                    if (!AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp))
                    {
                        return DenyAccess(job, httpApp, unauthorisedUrl, config.UnauthorisedAction);
                    }

                    return AllowAccess(httpApp);
                });
            }
            else if (config.UmbracoUserEnable && config.IpAddressesAccess == Enums.IpAddressesAccess.Restricted)
            {
                job.WatchWebRequests(regex, 75, (count, httpApp) =>
                {
                    var unauthorisedUrl = AccessHelper.UnauthorisedUrl(AllowKey, CacheLength, job, config.UrlType);

                    if (IsRequestAllowed(httpApp, unauthorisedUrl))
                    {
                        return WatchCycle.Continue;
                    }

                    var userIp = AccessHelper.ConvertToIpv6(httpApp.Context.Request.UserHostAddress);

                    if (!AccessHelper.IsRequestAuthenticatedUmbracoUser(httpApp) && !whiteList.Contains(userIp))
                    {
                        return DenyAccess(job, httpApp, unauthorisedUrl, config.UnauthorisedAction, userIp);
                    }

                    return AllowAccess(httpApp);
                });
            }
            else if (config.IpAddressesAccess == Enums.IpAddressesAccess.Restricted)
            {
                job.WatchWebRequests(regex, 75, (count, httpApp) =>
                {
                    var unauthorisedUrl = AccessHelper.UnauthorisedUrl(AllowKey, CacheLength, job, config.UrlType);

                    if (IsRequestAllowed(httpApp, unauthorisedUrl))
                    {
                        return WatchCycle.Continue;
                    }

                    var userIp = AccessHelper.ConvertToIpv6(httpApp.Context.Request.UserHostAddress);

                    if (!whiteList.Contains(userIp))
                    {
                        return DenyAccess(job, httpApp, unauthorisedUrl, config.UnauthorisedAction, userIp);
                    }

                    return AllowAccess(httpApp);
                });
            }

            return true;
        }
    }
}
