using Our.Shield.Core.Models;
using System;
using System.Net;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web;
using Umbraco.Core.Configuration;

namespace Our.Shield.Core.Helpers
{
    public static class AccessHelper
    {
        /// <summary>
        /// Checks if the current request is coming from an authenticated Umbraco User
        /// </summary>
        /// <param name="httpApp">The current HttpApplication</param>
        /// <returns>Whether or not the request is from an authenticated Umbraco User</returns>
        public static bool IsRequestAuthenticatedUmbracoUser(HttpApplication httpApp)
        {
            var httpContext = new HttpContextWrapper(httpApp.Context);
            var umbAuthTicket = httpContext.GetUmbracoAuthTicket();

            return httpContext.AuthenticateCurrentRequest(umbAuthTicket, true);
        }

        public static string UmbracoTrailingSlash(string originalUrl)
        {
            var url = string.Copy(originalUrl);
            var addTrailingSlash = UmbracoConfig.For.UmbracoSettings().RequestHandler.AddTrailingSlash;
            var lastChar = url[url.Length - 1];
            if (addTrailingSlash && lastChar != '\\')
            {
                url += "\\";
            }
            else if (!addTrailingSlash && lastChar == '\\')
            {
                url = url.Substring(0, url.Length - 1);
            }
            return url;
        }

    }
}
