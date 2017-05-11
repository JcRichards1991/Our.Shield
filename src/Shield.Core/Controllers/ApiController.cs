namespace Shield.Core.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;

    /// <summary>
    /// Api Controller for the Umbraco Access area of the custom section
    /// </summary>
    /// <example>
    /// Endpoint: /umbraco/backoffice/Shield/ShieldApi/{Action}
    /// </example>
    [PluginController(Constants.App.Name)]
    public class ShieldApiController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Api Endpoint for Posting the Umbraco Access Configuration.
        /// </summary>
        /// <param name="id">
        /// The Id of the configuration
        /// </param>
        /// <param name="model">
        /// The new configuration.
        /// </param>
        /// <param name="curUserId">
        /// Id of the currently logged in umbraco user
        /// </param>
        /// <example>
        /// Endpoint: /umbraco/backoffice/Shield/ShieldApi/PostConfiguration
        /// </example>
        /// <returns>
        /// Whether was successfully updated.
        /// </returns>
        [HttpPost]
        public bool PostConfiguration(string id, string model)
        {
            var op = Models.Operation<Models.Configuration>.Create(id);

            var curConfig = op.ReadConfiguration();

            Models.Configuration newConfig = Newtonsoft.Json.JsonConvert.DeserializeObject(model, curConfig.GetType()) as Models.Configuration;

            if (!op.Execute(newConfig))
            {
                // oh well, leave for polling to try and update
            }

            var curUmbracoUser = UmbracoContext.Security.CurrentUser;

            op.WriteJournal(new Models.Journal
            {
                Datestamp = DateTime.UtcNow,
                Message = $"{curUmbracoUser.Name} has updated configuration."
            });

            return op.WriteConfiguration(newConfig);
        }


        /// <summary>
        /// Api Endpoint for Getting the Umbraco Access Configuration.
        /// </summary>
        /// <returns>
        /// The configuration for the Umbraco Access area.
        /// </returns>
        [HttpGet]
        public Models.Configuration GetConfiguration(string id)
        {
            var op = Models.Operation<Models.Configuration>.Create(id);

            if(op == null)
            {
                return null;
            }

            return op.ReadConfiguration();
        }

        /// <summary>
        /// Api Endpoint for getting the Umbraco Access Journals
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Models.Journal> GetJournals(string id, int page, int itemsPerPage)
        {
            var op = Models.Operation<Models.Configuration>.Create(id);

            if(op == null)
            {
                return Enumerable.Empty<Models.Journal>();
            }

            return op.ReadJournals(page, itemsPerPage);
        }
    }
}
