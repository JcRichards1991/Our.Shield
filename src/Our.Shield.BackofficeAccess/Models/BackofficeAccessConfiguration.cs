using Newtonsoft.Json;
using Our.Shield.Core.Attributes;
using Our.Shield.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace Our.Shield.BackofficeAccess.Models
{
    /// <summary>
    /// The Backoffice Access Configuration
    /// </summary>
    [DebuggerDisplay("Enable: {Enabled}; Backend Access Url: {BackendAccessUrl}")]
    [JsonObject(MemberSerialization.OptIn)]
    public class BackofficeAccessConfiguration : AppConfiguration
    {
        /// <summary>
        /// The desired backoffice access URL
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
        [JsonProperty("transferUrlControl")]
        public TransferUrlControl TransferUrlControl { get; set; }
    }
}
