using System;
using Newtonsoft.Json;

namespace Shield.Core.Models
{
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
