using System;
using Newtonsoft.Json;

namespace Shield.Core.Models
{
    public interface IConfiguration
    {
        /// <summary>
        /// Gets or sets what environment this is for
        /// </summary>
        [JsonProperty("environment")]
        IEnvironment Environment { get; }

        /// <summary>
        /// Gets or sets what shield app is this for
        /// </summary>
        [JsonProperty("app")]
        IApp App { get; }

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
