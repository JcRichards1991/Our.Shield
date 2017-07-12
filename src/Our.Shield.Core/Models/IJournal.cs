namespace Our.Shield.Core.Models
{
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Journal interface
    /// </summary>
    public interface IJournal
    {
        /// <summary>
        /// App Id of the journal
        /// </summary>
        [JsonProperty("appId")]
        string AppId { get; }

        /// <summary>
        /// Environment Id of the journal
        /// </summary>
        [JsonProperty("environmentId")]
        int EnvironmentId { get; }

        /// <summary>
        /// Datestamp of when the journal was created
        /// </summary>
        [JsonProperty("datestamp")]
        DateTime Datestamp { get; }
    }
}
