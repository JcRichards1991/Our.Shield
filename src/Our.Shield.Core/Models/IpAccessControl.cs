using NetTools;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace Our.Shield.Core.Models
{
    public class IpAccessControl
    {
        public enum AccessTypes
        {
            [Description("Allow all client access except for specific ip addresses")]
            AllowAll = 0,

            [Description("Allow no client access except for these specific ip addresses")]
            AllowNone = 1
        }

        public enum IPAddressType
        {
            [Description("The Value is an IP Address")]
            Single,
            [Description("The Value is an IP Address Range")]
            Range
        }

        /// <summary>
        /// IP Address Model
        /// </summary>
        public class Entry
        {

            /// <summary>
            /// The Type of the IP addresses
            /// </summary>
            [JsonProperty("ipAddressType")]
            public IPAddressType IPAddressType { get; set; }

            /// <summary>
            /// Range or Ip Address with optional Cidr
            /// </summary>
            [JsonProperty("fromIpAddress")]
            public string FromIPAddress { get; set; }

            /// <summary>
            /// Range or Ip Address with optional Cidr
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

        /// <summary>
        /// What type of access is allowed
        /// </summary>
        [JsonProperty("accessType")]
        public AccessTypes AccessType { get; set; }

        /// <summary>
        /// List of exceptions to the access type
        /// </summary>
        [JsonProperty("exceptions")]
        public IEnumerable<Entry> Exceptions { get; set; }
    }
}
