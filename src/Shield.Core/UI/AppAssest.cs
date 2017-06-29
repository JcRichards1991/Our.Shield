using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shield.Core.UI
{
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
