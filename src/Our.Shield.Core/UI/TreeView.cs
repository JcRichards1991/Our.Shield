namespace Our.Shield.Core.UI
{
    using Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class TreeView
    {
        /// <summary>
        /// 
        /// </summary>
        public enum TreeViewType
        {
            [JsonProperty("environments")]
            Environments,

            [JsonProperty("environment")]
            Environment,

            [JsonProperty("app")]
            App
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("type")]
        public TreeViewType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("environments")]
        public IEnumerable<IEnvironment> Environments;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("environment")]
        public IEnvironment Environment;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("apps")]
        public IEnumerable<KeyValuePair<int, IApp>> Apps;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("app")]
        public IApp App;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("configuration")]
        public IConfiguration Configuration;

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("appAssests")]
        public AppAssest AppAssests { get; set; }
    }
}
