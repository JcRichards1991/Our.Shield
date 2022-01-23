using Our.Shield.Core.Models;
using System.Collections.Generic;
using System.Web;

namespace Our.Shield.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IIpAccessControlService
    {
        /// <summary>
        /// Return a list of ranges that contain invalid ranges
        /// </summary>
        /// <returns>List of errored ranges</returns>
        IEnumerable<string> InitIpAccessControl(IpAccessControl rule);

        /// <summary>
        /// States whether a specific IP address is valid within the rules of client access control
        /// </summary>
        /// <param name="rule">The IP Access Control to determine whether to grant access or not</param>
        /// <param name="request">The current HttpContext Request</param>
        /// <returns></returns>
        bool IsValid(IpAccessControl rule, HttpRequest request);

        /// <summary>
        /// Checks if the current request is coming from an authenticated Umbraco User
        /// </summary>
        /// <param name="httpApp">The current HttpApplication</param>
        /// <returns>Whether or not the request is from an authenticated Umbraco User</returns>
        bool IsRequestAuthenticatedUmbracoUser(HttpApplication httpApp);
    }
}
