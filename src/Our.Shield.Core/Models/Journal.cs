using Newtonsoft.Json;
using System;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Implements <see cref="IJournal"/>
    /// </summary>
    public class Journal : IJournal
    {
        /// <inheritdoc />
        [JsonProperty("environmentKey")]
        public Guid EnvironmentKey { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("appKey")]
        public Guid? AppKey { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("dateStamp")]
        public DateTime DateStamp { get; internal set; }

        /// <inheritdoc />
        [JsonProperty("message")]
        public string Message { get; internal set; }
    }
}
