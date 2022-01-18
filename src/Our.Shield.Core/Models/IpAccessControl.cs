using NetTools;
using Newtonsoft.Json;
using Our.Shield.Core.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace Our.Shield.Core.Models
{
    public class IpAccessControl
    {
        /// <summary>
        /// What type of access is allowed
        /// </summary>
        [JsonProperty("accessType")]
        public AccessTypes AccessType { get; set; }

        /// <summary>
        /// List of exceptions to the access type
        /// </summary>
        [JsonProperty("ipAccessRules")]
        public IEnumerable<IpAccessRule> IpAccessRules { get; set; }
    }
}
