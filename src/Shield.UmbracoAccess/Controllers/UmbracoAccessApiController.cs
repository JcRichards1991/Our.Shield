using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Shield.UmbracoAccess.Controllers
{
    /// <summary>
    /// Api Controller for the Umbraco Access area of the custom section
    /// </summary>
    [PluginController(Core.Constants.App.Name)]
    public class UmbracoAccessApiController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Api Endpoint for Posting the Umbraco Access Configuration.
        /// </summary>
        /// <param name="enable">
        /// Whether or not Umbraco Access is enabled
        /// </param>
        /// <param name="model">
        /// The new configuration.
        /// </param>
        /// <returns>
        /// Whether was successfully updated.
        /// </returns>
        [HttpPost]
        public bool PostConfiguration(bool enable, Operation.Configuration model)
        {
            return new Operation.Operation().Write(enable, model);
        }

        [HttpPost]
        public bool PostConfiguration(Operation.Configuration model)
        {
            return new Operation.Operation().Write(true, model);
        }


        /// <summary>
        /// Api Endpoint for Getting the Umbraco Access Configuration.
        /// </summary>
        /// <returns>
        /// The configuration for the Umbraco Access area.
        /// </returns>
        [HttpGet]
        public JsonResult GetConfiguration()
        {
            var configuration = new Operation.Operation().Read() as Operation.Configuration;

            if(configuration == null)
            {
                configuration = new Operation.Configuration
                {
                    BackendAccessUrl = ApplicationSettings.GetUmbracoPath(),
                    RedirectRewrite = (int)Enums.RedirectRewrite.Redirect,
                    UnauthorisedUrlType = (int)Enums.UnautorisedUrlType.String,
                    IpAddresses = new PropertyEditors.IpAddress.Models.IpAddress[0]
                };
            }

            var response = new JsonResult
            {
                Data = configuration
            };

            return response;
        }
    }
}
