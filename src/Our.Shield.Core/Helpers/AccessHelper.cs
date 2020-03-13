using System.Web;
using Umbraco.Core.Security;
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
            var umbracoAuthTicket = httpContext.GetUmbracoAuthTicket();
            if (umbracoAuthTicket != null)
            {
                httpContext.RenewUmbracoAuthTicket();
                return true;
            }
            return false;
        }
    }
}