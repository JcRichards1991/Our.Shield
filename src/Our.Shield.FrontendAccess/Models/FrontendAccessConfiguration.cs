using Newtonsoft.Json;
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
        /// White-listed IP Addresses that can access the backoffice access Url
        /// </summary>
        [JsonProperty("ipAddresses")]
        public IpEntry[] IpEntries { get; set; }

        /// <summary>
        /// Whether to redirect or rewrite the request to another location when frontend access is denied
        /// </summary>
        [JsonProperty("unauthorisedAction")]
        public Enums.UnauthorisedAction UnauthorisedAction { get; set; }

        /// <summary>
        /// The selector for the unauthorised url.
        /// </summary>
        [JsonProperty("unauthorisedUrlType")]
        public Enums.UrlType UnauthorisedUrlType { get; set; }

        /// <summary>
        /// The Url for the unauthorised access Url
        /// </summary>
        [JsonProperty("unauthorisedUrl")]
        public string UnauthorisedUrl { get; set; }

        /// <summary>
        /// The XPath to the content node for the unauthorised access Url
        /// </summary>
        [JsonProperty("unauthorisedUrlXPath")]
        public string UnauthorisedUrlXPath { get; set; }

        /// <summary>
        /// The Id/UID to the content node for the unauthorised access Url
        /// </summary>
        [JsonProperty("unauthorisedUrlContentPicker")]
        public string UnauthorisedUrlContentPicker { get; set; }
    }
}
