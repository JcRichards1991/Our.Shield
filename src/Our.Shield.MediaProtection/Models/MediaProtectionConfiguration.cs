using Newtonsoft.Json;
using Our.Shield.Core.Models;

namespace Our.Shield.MediaProtection.Models
{
    /// <inheritdoc />
    [JsonObject(MemberSerialization.OptIn)]
    public class MediaProtectionConfiguration : AppConfiguration
    {
        /// <summary>
        /// HotLinking Protection
        /// </summary>
        [JsonProperty("enableHotLinkingProtection")]
        public bool EnableHotLinkingProtection { get; set; }

        /// <summary>
        /// Directories that Hot Linking Protection is active on
        /// </summary>
        [JsonProperty("hotLinkingProtectedDirectories")]
        public string[] HotLinkingProtectedDirectories { get; set; }

        /// <summary>
        /// Member Only media
        /// </summary>
        [JsonProperty("enableMemberOnlyMedia")]
        public bool EnableMembersOnlyMedia { get; set; }
    }
}
