using Newtonsoft.Json;
using Our.Shield.Core.Models;

namespace Our.Shield.MediaProtection.Models
{
    /// <summary>
    /// 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MediaProtectionConfiguration : Configuration
    {
        /// <summary>
        /// HotLinking Protection
        /// </summary>
        [JsonProperty("enableHotLinkingProtection")]
        public bool EnableHotLinkingProtection { get; set; }

        /// <summary>
        /// Member Only media
        /// </summary>
        [JsonProperty("enableMemberOnlyMedia")]
        public bool EnableMembersOnlyMedia { get; set; }
    }
}
