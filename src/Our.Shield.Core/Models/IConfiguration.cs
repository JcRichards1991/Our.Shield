using Newtonsoft.Json;
using System;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// The Interface class for an App's Configuration
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Gets or sets whether the Configuration is Enabled.
        /// </summary>
        [JsonProperty("enable")]
        bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the configuration last modified data time
        /// </summary>
        [JsonProperty("lastModified")]
        DateTime? LastModified { get; }
    }
}
