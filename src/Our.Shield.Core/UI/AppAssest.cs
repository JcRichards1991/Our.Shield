namespace Our.Shield.Core.UI
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class AppAssest
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("view")]
        public string View { get; set; }

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
