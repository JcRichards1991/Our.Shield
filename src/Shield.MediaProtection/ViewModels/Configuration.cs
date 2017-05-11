namespace Shield.MediaProtection.ViewModels
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    public class Configuration : Core.Models.Configuration
    {
        /// <summary>
        /// Gets or sets the Media URL.
        /// </summary>
        [JsonProperty("backendAccessUrl")]
        public string MediaUrl { get; set; }

        /// <summary>
        /// Gets or sets the Collection of urls that can access media.
        /// </summary>
        [JsonProperty("backendAccessUrl")]
        public string[] ExceptionUrls { get; set; }
    }
}
