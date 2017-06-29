using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shield.Core.Models;

namespace Shield.Core.UI
{
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

    }
}
