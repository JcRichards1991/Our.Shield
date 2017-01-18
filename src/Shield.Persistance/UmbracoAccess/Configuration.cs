using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Shield.Persistance.UmbracoAccess
{
    /// <summary>
    /// The Umbraco Access Configuration serializable object for storing within the database.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration : Bal.IJsonValues
    {
        /// <summary>
        /// Gets or sets the Backend Access URL.
        /// </summary>
        [JsonProperty]
        public string BackendAccessUrl { get; set; }

        /// <summary>
        /// Gets or sets the Status Code.
        /// </summary>
        [JsonProperty]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or set the Ip Addresses.
        /// </summary>
        [JsonProperty]
        public IEnumerable<object> IpAddresses { get; set; }
    }
}
