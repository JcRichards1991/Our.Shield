namespace Shield.UmbracoAccess.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;

    /// <summary>
    /// Api Controller for the Umbraco Access area of the custom section
    /// </summary>
    [PluginController(Core.Constants.App.Name)]
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
        public bool PostConfiguration(ViewModels.Configuration model, int curUserId)
        {
            var op = new Models.Operation();
            var t = Core.Operation.Executor.Instance.Execute("");
            var curUmbracoUser = UmbracoContext.Application.Services.UserService.GetByProviderKey(curUserId);

            op.WriteJournal(new Core.Models.Journal {
                Datestamp = DateTime.UtcNow,
                Message = $"{curUmbracoUser.Name} has updated configuration."
            });

            return op.WriteConfiguration(model);
        }


        /// <summary>
        /// Api Endpoint for Getting the Umbraco Access Configuration.
        /// </summary>
        /// <returns>
        /// The configuration for the Umbraco Access area.
        /// </returns>
        [HttpGet]
        public System.Web.Http.Results.JsonResult<ViewModels.Configuration> GetConfiguration()
        {
            var configuration = new Models.Operation().ReadConfiguration() as ViewModels.Configuration;

            if(configuration == null)
            {
                configuration = new ViewModels.Configuration
                {
                    BackendAccessUrl = ApplicationSettings.GetUmbracoPath(),
                    RedirectRewrite = (int)Enums.RedirectRewrite.Redirect,
                    UnauthorisedUrlType = (int)Enums.UnautorisedUrlType.Url,
                    IpAddresses = new PropertyEditors.IpAddress.Models.IpAddress[0]
                };
            }

            return Json(configuration);
        }

        /// <summary>
        /// Api Endpoint for getting the Umbraco Access Journals
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Core.Models.Journal> GetJournals(int page, int itemsPerPage)
        {
            return new Models.Operation().ReadJournals(page, itemsPerPage) as IEnumerable<Core.Models.Journal>;
        }
    }
}
