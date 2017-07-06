namespace Our.Shield.Core.UI
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class AppAssest
    {
        [JsonProperty("view")]
        public string View { get; set; }

        [JsonProperty("stylesheets")]
        public IEnumerable<string> Stylesheets { get; set; }

        [JsonProperty("scripts")]
        public IEnumerable<string> Scripts { get; set; }
    }
}
