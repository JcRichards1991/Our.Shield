using Newtonsoft.Json;
using System;

namespace Our.Shield.Core.Models
{
    /// <inheritdoc />
    /// <summary>
    /// The Journal class to inherit from
    /// </summary>
    public abstract class Journal : IJournal
    {
        /// <inheritdoc />
        [JsonProperty("appId")]
        public string AppId { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("environmentId")]
        public int EnvironmentId { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("datestamp")]
        public DateTime Datestamp { get; internal set; }
    }
}
