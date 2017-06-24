using Newtonsoft.Json;

namespace Shield.MediaProtection.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration : Core.Persistance.Serialization.Configuration
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
