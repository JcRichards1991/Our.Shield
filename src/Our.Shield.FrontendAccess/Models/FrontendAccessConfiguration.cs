﻿using Newtonsoft.Json;
using Our.Shield.Core;
using Our.Shield.Core.Models;

namespace Our.Shield.FrontendAccess.Models
{
    public class FrontendAccessConfiguration : Configuration
    {
        /// <summary>
        /// Whether or not the Frontend can be accessed when the request is coming from an authenticated umbraco user
        /// </summary>
        [JsonProperty("umbracoUserEnable")]
        public bool UmbracoUserEnable { get; set; }

        /// <summary>
        /// Whether the Frontend is open to all IP addresses, or restricted to a white-list of IP addresses
        /// </summary>
        [JsonProperty("ipAddressesAccess")]
        public Enums.IpAddressesAccess IpAddressesAccess { get; set; }

        /// <summary>
        /// White-listed IP Addresses that can access the frontend
        /// </summary>
        [JsonProperty("ipAddresses")]
        public IpEntry[] IpEntries { get; set; }

        /// <summary>
        /// Where to send unauthorized users too
        /// </summary>
        [JsonProperty("unauthorized")]
        public TransferUrl Unauthorized { get; set; }
    }
}
