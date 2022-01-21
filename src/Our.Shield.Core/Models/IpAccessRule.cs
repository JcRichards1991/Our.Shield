using NetTools;
using Newtonsoft.Json;
using Our.Shield.Core.Enums;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// IP Address Model
    /// </summary>
    public class IpAccessRule
    {
        /// <summary>
        /// The Type of the IP addresses
        /// </summary>
        [JsonProperty("ipAddressType")]
        public IpAddressType IpAddressType { get; set; }

        /// <summary>
        /// Range or IP Address with optional Cidr
        /// </summary>
        [JsonProperty("fromIpAddress")]
        public string FromIpAddress { get; set; }

        /// <summary>
        /// Range or IP Address with optional Cidr
        /// </summary>
        [JsonProperty("toIpAddress")]
        public string ToIpAddress { get; set; }

        internal IPAddressRange Range { get; set; }

        /// <summary>
        /// Optional description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
