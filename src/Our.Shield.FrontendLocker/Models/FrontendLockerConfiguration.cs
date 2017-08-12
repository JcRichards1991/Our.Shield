using Newtonsoft.Json;
using Our.Shield.Core;
using Our.Shield.Core.Models;

namespace Our.Shield.FrontendLocker.Models
{
    public class FrontendLockerConfiguration : Configuration
    {
        /// <summary>
        /// Gets or sets the Status Code.
        /// </summary>
        [JsonProperty("unauthorisedAction")]
        public Enums.UnauthorisedAction UnauthorisedAction { get; set; }

        /// <summary>
        /// Gets or sets the Unauthorised URL.
        /// </summary>
        [JsonProperty("unauthorisedUrl")]
        public string UnauthorisedUrl { get; set; }
    }
}
