using Newtonsoft.Json;
using Our.Shield.Core.Models;
using System.Collections.Generic;

namespace Our.Shield.Core.UI
{
    public enum TreeViewType
    {
        Environments,
        Environment,
        App
    }

    public class TreeView
    {
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
        public IEnumerable<AppListingItem> Apps;
        
        [JsonProperty("app")]
        public IApp App;
        
        [JsonProperty("configuration")]
        public IAppConfiguration Configuration;
        
        [JsonProperty("journalListing")]
        public JournalListing JournalListing { get; set; }
        
        [JsonProperty("tabs")]
        public IEnumerable<Tab> Tabs { get; set; }

        [JsonProperty("appView")]
        public string AppView { get; set; }
    }
    
    public class Tab
    {
        [JsonProperty("label")]
        public string Caption { get; set; }

        [JsonProperty("id")]
        public int Id { get;set; }

        [JsonProperty("view")]
        public string View { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }
        
    public class JournalListingItem
    {
        [JsonProperty("datestamp")]
        public string Datestamp { get; set; }
        
        [JsonProperty("environment")]
        public IEnvironment Environment { get; set; }
        
        [JsonProperty("app")]
        public AppListingItem App { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class JournalListing
    {
        [JsonProperty("items")]
        public IEnumerable<JournalListingItem> Journals { get; set; }
        
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("type")]
        public TreeViewType Type { get; set; }
    }

    public class AppListingItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("appId")]
        public string AppId { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("icon")]
        public string Icon { get; set; }
        
        [JsonProperty("enable")]
        public bool Enable { get; set; }

        public AppListingItem() { }

        public AppListingItem(IJob job)
        {
            Id = job.Id;
            AppId = job.App.Id;
            Name = job.App.Name;
            Description = job.App.Description;
            Icon = job.App.Icon;
            Enable = job.ReadConfiguration().Enable;
        }
    }
}
