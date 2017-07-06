namespace Our.Shield.MediaProtection.Models
{
    using Core.Models;
    using Newtonsoft.Json;

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
