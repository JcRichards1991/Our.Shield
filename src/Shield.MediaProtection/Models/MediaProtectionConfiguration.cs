using Newtonsoft.Json;
using Shield.Core.Models;

namespace Shield.MediaProtection.Models
{
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
