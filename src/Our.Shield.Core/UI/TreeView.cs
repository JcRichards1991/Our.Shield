using Newtonsoft.Json;
using Our.Shield.Core.Models;
using System.Collections.Generic;

namespace Our.Shield.Core.UI
{
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
            Environment,
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
        public IEnumerable<AppListingItem> Apps;

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

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("journalListing")]
        public JournalListing JournalListing { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class JournalListingItem
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("datestamp")]
        public string Datestamp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("environment")]
        public IEnvironment Environment { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("app")]
        public AppListingItem App { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class JournalListing
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("items")]
        public IEnumerable<JournalListingItem> Journals { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AppListingItem
    {
        /// <summary>
        /// The Id of the tree node
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The Id of the app
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// The name of the app
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the app
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Icon of the app
        /// </summary>
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Whether or not the app is enabled
        /// </summary>
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
