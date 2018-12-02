using Newtonsoft.Json;
using Our.Shield.Core.Models;

namespace Our.Shield.Swagger.Models
{
    public class SwaggerConfiguration : AppConfiguration
    {
        /// <summary>
        /// Whether or not Swagger page can be accessed when the request is coming from an authenticated umbraco user and/or restrict via IP Address(es)
        /// </summary>
        [JsonProperty("umbracoUserEnable")]
        public bool UmbracoUserEnable { get; set; }

        /// <summary>
        /// Client access
        /// </summary>
        [JsonProperty("ipAccessRules")]
        public IpAccessControl IpAccessRules { get; set; }

        /// <summary>
        /// The Url Type Selector and the url
        /// </summary>
        [JsonProperty("unauthorized")]
        public TransferUrl Unauthorized { get; set; }
    }
}
