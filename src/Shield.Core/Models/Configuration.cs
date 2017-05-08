namespace Shield.Core.Models
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Configuration
    {

        /// <summary>
        /// Gets or sets whether the Configuration is Enabled.
        /// </summary>
        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }
}
