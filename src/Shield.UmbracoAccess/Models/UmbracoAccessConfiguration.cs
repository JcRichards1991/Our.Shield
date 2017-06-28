namespace Shield.UmbracoAccess.Models
{
    using Core.Models;
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    public class UmbracoAccessConfiguration : Configuration
    {
        /// <summary>
        /// Gets or sets the Backend Access URL.
        /// </summary>
        [JsonProperty("backendAccessUrl")]
        public string BackendAccessUrl { get; set; }

        /// <summary>
        /// Gets or sets the Status Code.
        /// </summary>
        [JsonProperty("redirectRewrite")]
        public Enums.RedirectRewrite RedirectRewrite { get; set; }

        /// <summary>
        /// Gets or set the Ip Addresses.
        /// </summary>
        [JsonProperty("ipAddresses")]
        public Models.IpAddress[] IpAddresses { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL by XPath.
        /// </summary>
        [JsonProperty("unauthorisedUrlXPath")]
        public string UnauthorisedUrlXPath { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL Type.
        /// </summary>
        [JsonProperty("unauthorisedUrlType")]
        public Enums.UnautorisedUrlType UnauthorisedUrlType { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL by Content Picker.
        /// </summary>
        [JsonProperty("unauthorisedUrlContentPicker")]
        public string UnauthorisedUrlContentPicker { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL.
        /// </summary>
        [JsonProperty("unauthorisedUrl")]
        public string UnauthorisedUrl { get; set; }
    }
}
