using Newtonsoft.Json;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// IP Address Model
    /// </summary>
    public class IpEntry
    {
        /// <summary>
        /// Gets or set the IP Address
        /// </summary>
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets a description for this IP Address 
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
