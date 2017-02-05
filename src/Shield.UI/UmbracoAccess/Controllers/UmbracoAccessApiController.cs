using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Shield.UI.UmbracoAccess.Controllers
{
    /// <summary>
    /// Api Controller for the Umbraco Access area of the custom section
    /// </summary>
    [PluginController(Constants.App.Name)]
    public class UmbracoAccessApiController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Api Endpoint for Posting the Umbraco Access Configuration.
        /// </summary>
        /// <param name="model">
        /// The new configuration.
        /// </param>
        /// <returns>
        /// Whether was successfully updated.
        /// </returns>
        [HttpPost]
        public bool PostConfiguration(Models.ViewModel model)
        {
            var db = new Persistance.UmbracoAccess.ConfigurationContext();

            var dataModel = new Persistance.UmbracoAccess.Configuration
            {
                BackendAccessUrl = model.backendAccessUrl,
                StatusCode = model.statusCode,
                IpAddresses = model.ipAddresses,
                UnauthorisedUrl = model.unauthorisedUrl,
                UnauthorisedUrlContentPicker = model.unauthorisedUrlContentPicker,
                UnauthorisedUrlType = (int)model.unauthorisedUrlType,
                UnauthorisedUrlXPath = model. unauthorisedUrlXPath
            };

            return db.Write(dataModel);
        }

        /// <summary>
        /// Api Endpoint for Getting the Umbraco Access Configuration.
        /// </summary>
        /// <returns>
        /// The configuration for the Umbraco Access area.
        /// </returns>
        [HttpGet]
        public Models.ViewModel GetConfiguration()
        {
            var db = new Persistance.UmbracoAccess.ConfigurationContext();

            var dataModel = db.Read();

            return new Models.ViewModel
            {
                backendAccessUrl = string.IsNullOrEmpty(dataModel.BackendAccessUrl) 
                    ? GetDefaultBackendAccessUrl() 
                    : dataModel.BackendAccessUrl,
                statusCode = dataModel.StatusCode == 0 ? Constants.Defaults.StatusCode : dataModel.StatusCode,
                unauthorisedUrl = string.IsNullOrEmpty(dataModel.UnauthorisedUrl) ? Constants.Defaults.UnauthorisedUrl : dataModel.UnauthorisedUrl,
                unauthorisedUrlXPath = string.IsNullOrEmpty(dataModel.UnauthorisedUrlXPath) ? string.Empty : dataModel.UnauthorisedUrlXPath,
                unauthorisedUrlContentPicker = string.IsNullOrEmpty(dataModel.UnauthorisedUrlContentPicker) ? string.Empty : dataModel.UnauthorisedUrlContentPicker,
                ipAddresses = dataModel.IpAddresses ?? Enumerable.Empty<string>(),
                unauthorisedUrlType = dataModel.UnauthorisedUrlType == 0
                    ? Enums.UnautorisedUrlType.String
                    : dataModel.UnauthorisedUrlType == 1
                        ? Enums.UnautorisedUrlType.XPath
                        : Enums.UnautorisedUrlType.ContentPicker
            };
        }

        private string GetDefaultBackendAccessUrl()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["umbracoPath"]))
                return Constants.Defaults.BackendAccessUrl;
            return ConfigurationManager.AppSettings["umbracoPath"];
        }
    }
}
