using Newtonsoft.Json;
using Our.Shield.Core;
using Our.Shield.Core.Models;

namespace Our.Shield.EnvironmentLocker.Models
{
    public class EnvironmentLockerConfiguration : Configuration
    {
        /// <summary>
        /// Gets or sets the Status Code.
        /// </summary>
        [JsonProperty("unauthorisedAction")]
        public Enums.UnauthorisedAction UnauthorisedAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("unauthorisedUrlType")]
        public Enums.UnautorisedUrlType UnauthorisedUrlType { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL by XPath.
        /// </summary>
        [JsonProperty("unauthorisedUrlXPath")]
        public string UnauthorisedUrlXPath { get; set; }

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
