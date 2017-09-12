using Newtonsoft.Json;
using Our.Shield.Core;
using Our.Shield.Core.Models;

namespace Our.Shield.Elmah.Models
{
    public class ElmahConfiguration : Configuration
    {
        /// <summary>
        /// Whether or not ELMAH reporting page can be accessed when the request is coming from an authenticated umbraco user and/or restrict via IP Address(es)
        /// </summary>
        [JsonProperty("umbracoUserEnable")]
        public bool UmbracoUserEnable { get; set; }

        /// <summary>
        /// Whether the ELMAH reporting page is open to all IP addresses, or restricted to a white-list of IP addresses
        /// </summary>
        [JsonProperty("ipAddressesAccess")]
        public Enums.IpAddressesAccess IpAddressesAccess { get; set; }

        /// <summary>
        /// White-listed IP Addresses that can access the ELMAH reporting page
        /// </summary>
        [JsonProperty("ipAddresses")]
        public IpEntry[] IpEntries { get; set; }

        /// <summary>
        /// Whether to redirect or rewrite the request to another location when ELMAH reporting page is denied
        /// </summary>
        [JsonProperty("unauthorisedAction")]
        public Enums.UnauthorisedAction UnauthorisedAction { get; set; }

        /// <summary>
        /// The URL Type Selector and the URL
        /// </summary>
        [JsonProperty("umbracoUrl")]
        public TransferUrl UmbracoUrl { get; set; }
    }
}
