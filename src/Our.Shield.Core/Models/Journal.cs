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
        [JsonIgnore]
        public string AppId { get; internal set; }

        /// <summary>
        /// Environment Id of the journal
        /// </summary>
        [JsonIgnore]
        public int EnvironmentId { get; }

        /// <summary>
        /// Datestamp of when the journal was created
        /// </summary>
        [JsonIgnore]
        public DateTime Datestamp { get; }
    }
}
