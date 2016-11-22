using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Shield.UI.UmbracoAccess.Controllers
{
    [PluginController(Constants.App.Name)]
    public class UmbracoAccessApiController : UmbracoAuthorizedJsonController
    {
        private Helpers.IPAddressAndDomainRestrictionsHelper ModuleHelper = new Helpers.IPAddressAndDomainRestrictionsHelper();

        /// <summary>
        /// Adds an IP to the IP Address and Domain Restriction module
        /// within IIS for the Umbraco folder with Allow / Deny access
        /// </summary>
        /// <param name="ip">The IP Object</param>
        /// <returns>Whther or not was successfully added</returns>
        [HttpPost]
        public void Post(Models.IP ip)
        {
            ModuleHelper.AddIpAddress(ip);
        }

        /// <summary>
        /// Get's an IP address by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public Models.IP Get(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get's all IP addresses
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Models.IP> Get()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an IP by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public void Delete (string name)
        {
            throw new NotImplementedException();
        }
    }
}
