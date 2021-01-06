using Newtonsoft.Json;
using System.Collections.Generic;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AppAssest
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("stylesheets")]
        public IEnumerable<string> Stylesheets { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("scripts")]
        public IEnumerable<string> Scripts { get; set; }
    }
}
