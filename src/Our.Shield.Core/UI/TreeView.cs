namespace Our.Shield.Core.UI
{
    using Models;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class TreeView
    {
        public enum TreeViewType
        {
            [JsonProperty("environments")]
            Environments,

            [JsonProperty("environment")]
            Environment,

            [JsonProperty("app")]
            App
        }

        [JsonProperty("type")]
        public TreeViewType Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("environments")]
        public IEnumerable<IEnvironment> Environments;

        [JsonProperty("environment")]
        public IEnvironment Environment;

        [JsonProperty("apps")]
        public IEnumerable<IApp> Apps;

        [JsonProperty("app")]
        public IApp App;

        [JsonProperty("configuration")]
        public IConfiguration Configuration;

        [JsonProperty("appAssests")]
        public AppAssest AppAssests { get; set; }
    }
}
