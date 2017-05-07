using Newtonsoft.Json;

namespace Shield.Core.Models
{
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
