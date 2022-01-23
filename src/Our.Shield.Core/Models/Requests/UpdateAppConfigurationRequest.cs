using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Our.Shield.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Our.Shield.Core.Models.Requests
{
    /// <summary>
    /// Request model for updating an App's configuration
    /// </summary>
    public class UpdateAppConfigurationRequest
    {
        /// <summary>
        /// The App Id of the App updating
        /// </summary>
        [Required]
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// The Key of the App to update the configuration for
        /// </summary>
        [NotEmpty]
        [JsonProperty("key")]
        public Guid Key { get; set; }

        /// <summary>
        /// The Environment's Key that the App's configuration is being updated for
        /// </summary>
        [NotEmpty]
        [JsonProperty("environmentKey")]
        public Guid EnvironmentKey { get; set; }

        /// <summary>
        /// The new configuration
        /// </summary>
        [Required]
        [JsonProperty("configuration")]
        public JObject Configuration { get; set; }
    }
}
