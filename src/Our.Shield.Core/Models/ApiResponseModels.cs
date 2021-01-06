using Newtonsoft.Json;
using Our.Shield.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Our.Shield.Core.Models
{
    public class AppApiResponseModel : AppListingItem
    {
        public AppApiResponseModel(IJob job) : base(job) { }

        [JsonProperty("configuration")]
        public IAppConfiguration Configuration { get; set; }

        [JsonProperty("environment")]
        public IEnvironment Environment { get; set; }

        [JsonProperty("tabs")]
        public IEnumerable<ITab> Tabs { get; set; }
    }

    public class EnvironmentApiResponseModel : Environment
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("apps")]
        public IEnumerable<AppListingItem> Apps;

        public EnvironmentApiResponseModel(IEnvironment environment)
        {
            Id = environment.Id;
            Key = environment.Key;
            Name = environment.Name;
            Icon = environment.Icon;
            Domains = environment.Domains;
            SortOrder = environment.SortOrder;
            Enable = environment.Enable;
            ContinueProcessing = environment.ContinueProcessing;
            ColorIndicator = environment.ColorIndicator;
        }
    }

    public interface ITab
    {
        [JsonProperty("label")]
        string Caption { get; }

        [JsonProperty("id")]
        int Id { get; }

        [JsonProperty("view")]
        string View { get; }

        [JsonProperty("active")]
        bool Active { get; }
    }

    public class Tab : ITab
    {
        public Tab(AppTabAttribute attr)
        {
            View = attr.FilePath;
            Caption = attr.Caption;
            Id = attr.SortOrder;
            Active = attr.SortOrder == 0;
        }

        public string Caption { get; set; }
        
        public int Id { get;set; }
        
        public string View { get; set; }
        
        public bool Active { get; set; }
    }

    public class AppConfigTab : Tab
    {
        public AppConfigTab(AppEditorAttribute attr) : base(attr)
        {
            ConfigView = attr.AppView;
        }

        [JsonProperty("configView")]
        public string ConfigView { get; set; }
    }
        
    public class JournalListingItem
    {
        [JsonProperty("dateStamp")]
        public string DateStamp { get; set; }
        
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
    }

    public class AppListingItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("key")]
        public Guid Key { get; set; }
        
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
            Key = job.Key;
            AppId = job.App.Id;
            Name = job.App.Name;
            Description = job.App.Description;
            Icon = job.App.Icon;
            Enable = job.ReadConfiguration().Enable;
        }
    }
}
