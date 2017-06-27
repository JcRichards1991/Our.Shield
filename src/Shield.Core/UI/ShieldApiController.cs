namespace Shield.Core.UI
{
    using System.Linq;
    using System.Web.Http;
    using Umbraco.Web.Editors;
    using Umbraco.Web.Mvc;
    using Newtonsoft.Json.Linq;
    using Shield.Core.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Api Controller for the Umbraco Access area of the custom section
    /// </summary>
    /// <example>
    /// Endpoint: /umbraco/backoffice/Shield/ShieldApi/{Action}
    /// </example>
    [PluginController(Constants.App.Alias)]
    public class ShieldApiController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Api Endpoint for Posting the Umbraco Access Configuration.
        /// </summary>
        /// <param name="id">
        /// The Id of the configuration
        /// </param>
        /// <param name="model">
        /// The new configuration settings.
        /// </param>
        /// <example>
        /// Endpoint: /umbraco/backoffice/Shield/ShieldApi/Configuration?id=[id]
        /// </example>
        /// <returns>
        /// Whether was successfully updated.
        /// </returns>
        [HttpPost]
        public new bool Configuration(int id, [FromBody] JObject model)
        {
            //var curUmbracoUser = UmbracoContext.Security.CurrentUser;
            var job = Operation.JobService.Instance.Job(id);

            var config = model.ToObject(((Job) job).ConfigType) as IConfiguration;

            return job.WriteConfiguration(config);
        }


        /// <summary>
        /// Api Endpoint for Getting the Umbraco Access Configuration.
        /// </summary>
        /// <param name="id">
        /// Id Of the configuration to return
        /// </param>
        /// <example>
        /// Endpoint: /umbraco/backoffice/Shield/ShieldApi/Configuration?id={Id}
        /// </example>
        /// <returns>
        /// The configuration for the Umbraco Access area.
        /// </returns>
        [HttpGet]
        public new IConfiguration Configuration(int id)
        {
            var job = Operation.JobService.Instance.Job(id);
            return job.ReadConfiguration();
        }

        /// <summary>
        /// Api Endpoint for getting the Umbraco Access Journals
        /// </summary>
        /// <param name="id">
        /// Id of the configuration to return journals for
        /// </param>
        /// <param name="page">
        /// The page of results to return
        /// </param>
        /// <param name="itemsPerPage">
        /// Number of items per page
        /// </param>
        /// <returns>
        /// Collection of journals for the desired configuration
        /// </returns>
        [HttpGet]
        public IEnumerable<IJournal> Journals(int id, int page, int itemsPerPage)
        {
            var job = Operation.JobService.Instance.Job(id);
            return job.ListJournals<Journal>(page, itemsPerPage);
        }
    }
}
