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
