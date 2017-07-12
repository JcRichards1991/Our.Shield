namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// The Journal class to inherit from
    /// </summary>
    public abstract class Journal : IJournal
    {
        /// <summary>
        /// App Id of the journal
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; internal set; }

        /// <summary>
        /// Environment Id of the journal
        /// </summary>
        [JsonProperty("environmentId")]
        public int EnvironmentId { get; internal set; }

        /// <summary>
        /// Datestamp of when the journal was created
        /// </summary>
        [JsonProperty("datestamp")]
        public DateTime Datestamp { get; internal set; }
    }
}
