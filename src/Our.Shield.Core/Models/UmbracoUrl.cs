using Newtonsoft.Json;
using Our.Shield.Core.Enums;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UmbracoUrl
    {
        /// <summary>
        /// The type of URL
        /// </summary>
        [JsonProperty("type")]
        public UmbracoUrlType Type { get; set; }

        /// <summary>
        /// The value of the URL
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
