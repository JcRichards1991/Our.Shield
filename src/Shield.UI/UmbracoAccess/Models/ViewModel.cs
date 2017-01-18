using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;

namespace Shield.UI.UmbracoAccess.Models
{
    /// <summary>
    /// Umbraco Access View Model for the edit view.
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// Gets or sets the Backend Access URL.
        /// </summary>
        public string backendAccessUrl { get; set; }

        /// <summary>
        /// Gets or sets the Status Code
        /// </summary>
        public string statusCode { get; set; }

        /// <summary>
        /// Gets or sets the Ip Addresses.
        /// </summary>
        public IEnumerable<object> ipAddresses { get; set; }
    }
}
