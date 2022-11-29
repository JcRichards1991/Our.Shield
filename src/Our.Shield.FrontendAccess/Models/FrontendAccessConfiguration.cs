using Newtonsoft.Json;
using Our.Shield.Core.Models;

namespace Our.Shield.FrontendAccess.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class FrontendAccessConfiguration : AppConfiguration
    {
        /// <summary>
        /// Whether or not the Frontend can be accessed when the request is coming from an authenticated umbraco user
        /// </summary>
        [JsonProperty("umbracoUserEnable")]
        public bool UmbracoUserEnable { get; set; }

        /// <summary>
        /// Client access
        /// </summary>
        [JsonProperty("ipAccessControl")]
        public IpAccessControl IpAccessControl { get; set; }

        /// <summary>
        /// Where to send unauthorized users too
        /// </summary>
        [JsonProperty("transferUrlControl")]
        public TransferUrlControl TransferUrlControl { get; set; }
    }
}
