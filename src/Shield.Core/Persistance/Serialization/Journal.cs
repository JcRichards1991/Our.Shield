namespace Shield.Core.Persistance.Serialization
{
    using System;
    using Newtonsoft.Json;

    internal class Journal
    {
        /// <summary>
        /// Gets or sets what environment this is for
        /// </summary>
        [JsonIgnore]
        public int EnvironmentId { get; set; }

        /// <summary>
        /// Gets or sets what sheild app is this for
        /// </summary>
        [JsonIgnore]
        public int AppId { get; set; }

        /// <summary>
        /// The Date stamp of the journal
        /// </summary>
        [JsonIgnore]
        public DateTime Datestamp { get; set; }

        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        public string Message { get; set; }
    }
}
