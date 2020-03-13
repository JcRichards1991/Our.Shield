using Newtonsoft.Json;
using Our.Shield.Core.Models;
using System.Collections.Generic;

namespace Our.Shield.BackofficeAccess.Persistence.Models
{
    public class Configuration103
    {
        [JsonProperty("backendAccessUrl")]
        public string BackendAccessUrl { get; set; }

        [JsonProperty("ipAddressesAccess")]
        public int IpAddressesAccess { get; set; }

        [JsonProperty("IpAddresses")]
        public IEnumerable<IpEntry103> IpAddresses { get; set; }

        [JsonProperty("unauthorisedAction")]
        public TransferTypes UnauthorisedAction { get; set; }

        [JsonProperty("urlType")]
        public UrlType103 UrlType { get; set; }
    }

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
