using Newtonsoft.Json;
using System;

namespace Our.Shield.Core.Models
{
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

        /// <summary>
        /// The message of the Journal
        /// </summary>
        [JsonProperty("message")]
        string Message { get; }
    }
}
