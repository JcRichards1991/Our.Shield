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
        /// Environment Id of the journal
        /// </summary>
        [JsonProperty("environmentId")]
        Guid EnvironmentKey { get; }

        /// <summary>
        /// App Id of the journal
        /// </summary>
        [JsonProperty("appId")]
        Guid? AppKey { get; }

        /// <summary>
        /// Date stamp of when the journal was created
        /// </summary>
        [JsonProperty("dateStamp")]
        DateTime DateStamp { get; }

        /// <summary>
        /// The message of the Journal
        /// </summary>
        [JsonProperty("message")]
        string Message { get; }
    }
}
