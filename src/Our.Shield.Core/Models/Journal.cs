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

        /// <inheritdoc />
        [JsonProperty("message")]
        public string Message { get; internal set; }

        /// <summary>
        /// Initialises a JournalMessage Object with the desired message
        /// </summary>
        /// <param name="message">The message for the journal</param>
        protected Journal(string message)
        {
            Message = message;
        }
    }
}
