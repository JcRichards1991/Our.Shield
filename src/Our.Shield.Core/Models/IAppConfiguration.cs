using Newtonsoft.Json;
using System;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Defines the properties that every configuration requires
    /// </summary>
    public interface IAppConfiguration
    {
        /// <summary>
        /// Gets or sets whether the Configuration is Enabled.
        /// </summary>
        [JsonProperty("enabled")]
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the configuration last modified date time
        /// </summary>
        [JsonProperty("lastModified")]
        DateTime? LastModifiedDateUtc { get; set; }
    }
}
