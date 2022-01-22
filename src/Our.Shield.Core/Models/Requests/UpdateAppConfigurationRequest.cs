using Newtonsoft.Json.Linq;
using System;

namespace Our.Shield.Core.Models.Requests
{
    /// <summary>
    /// Request model for updating an App's configuration
    /// </summary>
    public class UpdateAppConfigurationRequest
    {
        /// <summary>
        /// The Key of the App to update the configuration for
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// The App Id of the App updating
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// The new configuration
        /// </summary>
        public JObject Configuration { get; set; }
    }
}
