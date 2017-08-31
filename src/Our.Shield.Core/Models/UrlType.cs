using Newtonsoft.Json;

namespace Our.Shield.Core.Models
{
    public class UrlType
    {
        /// <summary>
        /// The selector for the url
        /// </summary>
        [JsonProperty("urlSelector")]
        public Enums.UrlType UrlSelector { get; set; }

        /// <summary>
        /// The Url for the Url
        /// </summary>
        [JsonProperty("strUrl")]
        public string StrUrl { get; set; }

        /// <summary>
        /// The XPath to the content node for the Url
        /// </summary>
        [JsonProperty("xpathUrl")]
        public string XpathUrl { get; set; }

        /// <summary>
        /// The Id/UID to the content node for the Url
        /// </summary>
        [JsonProperty("contentPickerUrl")]
        public string ContentPickerUrl { get; set; }
    }
}
