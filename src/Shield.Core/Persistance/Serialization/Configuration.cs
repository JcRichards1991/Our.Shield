namespace Shield.Core.Persistance.Serialization
{
    using System;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    internal abstract class Configuration
    {
        /// <summary>
        /// Gets or sets whether the Configuration is Enabled.
        /// </summary>
        [JsonIgnore]
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the configuration last modified data time
        /// </summary>
        [JsonIgnore]
        public DateTime? LastModified { get; set; }
    }
}
