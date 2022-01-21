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
        [JsonProperty("appKey")]
        public Guid AppKey { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("environmentKey")]
        public Guid EnvironmentKey { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("dateStamp")]
        public DateTime DateStamp { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("message")]
        public string Message { get; internal set; }

        /// <summary>
        /// Initializes a JournalMessage Object with the desired message
        /// </summary>
        /// <param name="message">The message for the journal</param>
        protected Journal(string message)
        {
            Message = message;
        }
    }
}
