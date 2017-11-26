using Newtonsoft.Json;
using Our.Shield.Core.Models;

namespace Our.Shield.FrontendAccess.Persistence.Models
{
    public class IpEntry103
    {
        [JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class UrlType103
    {
        [JsonProperty("urlSelector")]
        public UmbracoUrlTypes UrlSelector { get; set; }
        
        [JsonProperty("strUrl")]
        public string StrUrl { get; set; }
        
        [JsonProperty("xpathUrl")]
        public string XPathUrl { get; set; }

        [JsonProperty("contentPickerUrl")]
        public string ContentPickerUrl { get; set; }
    }
}
