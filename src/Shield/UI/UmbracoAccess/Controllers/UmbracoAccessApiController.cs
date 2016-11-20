using System;
using System.Collections.Generic;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Shield.UI.UmbracoAccess.Controllers
{
    [PluginController(Constants.App.Name)]
    public class UmbracoAccessApiController : UmbracoAuthorizedJsonController
    {
        private IIS.Helpers.IPAddressAndDomainRestrictionsHelper IISModuleHelper = new IIS.Helpers.IPAddressAndDomainRestrictionsHelper();

        /// <summary>
        /// Adds an IP to the IP Address and Domain Restriction module
        /// within IIS for the Umbraco folder with Allow / Deny access
        /// </summary>
        /// <param name="ip">The IP Object</param>
        /// <returns>Whther or not was successfully added</returns>
        public bool Post(Models.IP ip)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Models.IP Get(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Models.IP> Get()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Delete (string name)
        {
            throw new NotImplementedException();
        }
    }
}
