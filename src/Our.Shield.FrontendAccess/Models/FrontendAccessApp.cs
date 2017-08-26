using Our.Shield.Core;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using Our.Shield.Core.Operation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web;

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
                    UnauthorisedAction = Enums.UnauthorisedAction.Redirect
                };
            }
        }

        private readonly string allowKey = Guid.NewGuid().ToString();

        private IPAddress ConvertToIpv6(string ip)
        {
            if (ip.Equals("127.0.0.1"))
                ip = "::1";

            IPAddress typedIp;
            if (IPAddress.TryParse(ip, out typedIp))
            {
                return typedIp.MapToIPv6();
            }

            return null;
        }

        private readonly TimeSpan CacheLength = new TimeSpan(TimeSpan.TicksPerDay); 

        private string UnauthorisedUrl(IJob job, FrontendAccessConfiguration config)
        {
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(allowKey, () =>
            {
                string url = null;
                var journalMessage = new JournalMessage();
                var umbContext = UmbracoContext.Current;

                switch (config.UnauthorisedUrlType)
                {
                    case Enums.UrlType.Url:
                        if (!string.IsNullOrEmpty(config.UnauthorisedUrl))
                        {
                            url = config.UnauthorisedUrl;
                            break;
                        }

                        journalMessage.Message = "Error: No Unauthorized URL set in configuration";
                        break;

                    case Enums.UrlType.XPath:
                        var xpathNode = umbContext.ContentCache.GetSingleByXPath(config.UnauthorisedUrlXPath);

                        if (xpathNode != null)
                        {
                            url = xpathNode.Url;
                            break;
                        }

                        journalMessage.Message = "Error: Unable to get the unauthorized URL from the specified XPath expression";
                        break;

                    case Enums.UrlType.ContentPicker:
                        int id;

                        if (int.TryParse(config.UnauthorisedUrlContentPicker, out id))
                        {
                            var contentPickerNode = umbContext.ContentCache.GetById(id);

                            if (contentPickerNode != null)
                            {
                                url = contentPickerNode.Url;
                                break;
                            }

                            journalMessage.Message = "Error: Unable to get the unauthorized URL from the unauthorized URL content picker. Please ensure the selected page is published and hasn't been deleted";
                            break;
                        }

                        journalMessage.Message = "Error: Unable to parse the selected unauthorized URL content picker item. Please ensure a valid content node is selected";
                        break;

                    default:
                        journalMessage.Message = "Error: Unable to determine which method to use to get the unauthorized URL. Please ensure URL, XPath or Content Picker is selected";
                        break;
                }

                if (url == null)
                {
                    if (!string.IsNullOrEmpty(journalMessage.Message))
                    {
                        job.WriteJournal(journalMessage);
                    }

                    return null;
                }

                return url;
            }, CacheLength) as string;
        }

        private bool IsRequestAllowed(HttpApplication httpApp, string url)
        {
            if (httpApp.Context.Request.Url.AbsolutePath.Equals(url) || (bool?)httpApp.Context.Items[allowKey] == true)
            {
                return true;
            }

            return false;
        }

        private bool IsAuthenticatedUmbracoUser(HttpApplication httpApp)
        {
            var httpContext = new HttpContextWrapper(httpApp.Context);
            var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

            return httpContext.AuthenticateCurrentRequest(umbAuthTicket, true);
        }

        private WatchCycle DenyAccess(IJob job, HttpApplication httpApp, IPAddress userIp, string unauthorisedUrl, Enums.UnauthorisedAction action)
        {
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
            httpApp.Context.Items.Add(allowKey, true);
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
            job.UnwatchWebRequests();

            if (!c.Enable || !job.Environment.Enable)
            {
                return true;
            }

            var config = c as FrontendAccessConfiguration;
            var hardUmbracoLocation = ApplicationSettings.UmbracoPath;
            var regex = new Regex("^/$|^(/(?!" + hardUmbracoLocation.Trim('/') + ")[\\w-/_]+?)$", RegexOptions.IgnoreCase);

            var whiteList = new List<IPAddress>();

            foreach (var ipEntry in config.IpEntries)
            {
                var ip = ConvertToIpv6(ipEntry.IpAddress);
                if (ip == null)
                {
                    job.WriteJournal(new JournalMessage($"Error: Invalid IP Address {ipEntry.IpAddress}, unable to add to white-list"));
                    continue;
                }

                whiteList.Add(ip);
            }

            if (config.UmbracoUserEnable || config.IpAddressesAccess == Enums.IpAddressesAccess.Restricted)
            {
                job.WatchWebRequests(regex, 75, (count, httpApp) =>
                {
                    var unauthorisedUrl = UnauthorisedUrl(job, config);

                    if (IsRequestAllowed(httpApp, unauthorisedUrl))
                    {
                        return WatchCycle.Continue;
                    }

                    var userIp = ConvertToIpv6(httpApp.Context.Request.UserHostAddress);

                    if (config.UmbracoUserEnable && config.IpAddressesAccess == Enums.IpAddressesAccess.Unrestricted)
                    {
                        if (!IsAuthenticatedUmbracoUser(httpApp))
                        {
                            return DenyAccess(job, httpApp, userIp, unauthorisedUrl, config.UnauthorisedAction);
                        }
                    }
                    else if (config.UmbracoUserEnable && config.IpAddressesAccess == Enums.IpAddressesAccess.Restricted)
                    {
                        if (!IsAuthenticatedUmbracoUser(httpApp) && !whiteList.Contains(userIp))
                        {
                            return DenyAccess(job, httpApp, userIp, unauthorisedUrl, config.UnauthorisedAction);
                        }
                    }
                    else if (config.IpAddressesAccess == Enums.IpAddressesAccess.Restricted)
                    {
                        if (!whiteList.Contains(userIp))
                        {
                            return DenyAccess(job, httpApp, userIp, unauthorisedUrl, config.UnauthorisedAction);
                        }
                    }

                    return AllowAccess(httpApp);
                });
            }

            return true;
        }
    }
}
