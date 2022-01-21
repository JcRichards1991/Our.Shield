using Newtonsoft.Json;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using System.Collections.Generic;

namespace Our.Shield.BackofficeAccess.Models
{
    /// <inheritdoc />
    /// <summary>
    /// The Backoffice Access Configuration
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BackofficeAccessConfiguration : AppConfiguration
    {
        /// <summary>
        /// The desired backoffice access url
        /// </summary>
        [JsonProperty("backendAccessUrl")]
        [SingleEnvironment]
        public string BackendAccessUrl { get; set; }

        [JsonProperty("excludeUrls")]
        [SingleEnvironment]
        public IEnumerable<string> ExcludeUrls { get; set; }

        /// <summary>
        /// Client access
        /// </summary>
        [JsonProperty("ipAccessControl")]
        public IpAccessControl IpAccessControl { get; set; }

        /// <summary>
        /// The URL Type Selector and the URL
        /// </summary>
        [JsonProperty("unauthorized")]
        public TransferUrlControl Unauthorized { get; set; }
    }
}
