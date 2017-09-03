using Our.Shield.Core.Models;
using System;
using System.Net;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web;

namespace Our.Shield.Core.Helpers
{
    public static class AccessHelper
    {
        /// <summary>
        /// Gets the Unauthorised Url from the Selector and stores it within the cache
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="cacheLength">The length of time to store within the cache</param>
        /// <param name="job">The job handling the request</param>
        /// <param name="urlType">The Url Type object from the app's config</param>
        /// <returns>The Unauthorised Url, or null</returns>
        public static string UnauthorisedUrl(string cacheKey, TimeSpan cacheLength, IJob job, UrlType urlType)
        {
            return ApplicationContext.Current.ApplicationCache.RuntimeCache.GetCacheItem(cacheKey, () =>
            {
                string url = null;
                var journalMessage = new JournalMessage();
                var umbContext = UmbracoContext.Current;

                switch (urlType.UrlSelector)
                {
                    case Enums.UrlType.Url:
                        if (!string.IsNullOrEmpty(urlType.StrUrl))
                        {
                            url = urlType.StrUrl;
                            break;
                        }

                        journalMessage.Message = "Error: No Unauthorized URL set in configuration";
                        break;

                    case Enums.UrlType.XPath:
                        var xpathNode = umbContext.ContentCache.GetSingleByXPath(urlType.XpathUrl);

                        if (xpathNode != null)
                        {
                            url = xpathNode.Url;
                            break;
                        }

                        journalMessage.Message = "Error: Unable to get the unauthorized URL from the specified XPath expression";
                        break;

                    case Enums.UrlType.ContentPicker:
                        //  By default, Content Picker uses Id and not UID, though may change in future
                        //  So, for now, let's leave this code as is :)

                        int id;

                        if (int.TryParse(urlType.ContentPickerUrl, out id))
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
            }, cacheLength) as string;
        }

        /// <summary>
        /// Checks if the current request is coming from an authenticated Umbraco User
        /// </summary>
        /// <param name="httpApp">The current HttpApplication</param>
        /// <returns>Whther or not the request is from an authenticated Umbraco User</returns>
        public static bool IsRequestAuthenticatedUmbracoUser(HttpApplication httpApp)
        {
            var httpContext = new HttpContextWrapper(httpApp.Context);
            var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

            return httpContext.AuthenticateCurrentRequest(umbAuthTicket, true);
        }

        /// <summary>
        /// Tries to convert a string ip address to System.Net.IPAddress object
        /// </summary>
        /// <param name="ip">The ip address to convert</param>
        /// <returns>The ip address as IPv6 or null if failed to convert</returns>
        public static IPAddress ConvertToIpv6(string ip)
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
    }
}
