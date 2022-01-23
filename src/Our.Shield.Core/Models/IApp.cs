using Newtonsoft.Json;
using System;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Definition of an App to plug-in to Our.Shield custom umbraco section
    /// </summary>
    public interface IApp : IFrisk
    {
        /// <summary>
        /// The Unique key of the App
        /// </summary>
        [JsonProperty("key")]
        Guid Key { get; set; }

        /// <summary>
        /// CSS class of icon
        /// </summary>
        [JsonProperty("icon")]
        string Icon { get; }

        /// <summary>
        /// The default configuration for the App
        /// </summary>
        [JsonIgnore]
        IAppConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// The initialize method for the App
        /// </summary>
        /// <returns>True if successfully initialized; Otherwise, False</returns>
        bool Init();

        /// <summary>
        /// Execute the config of a derived App
        /// </summary>
        /// <param name="job">The job to execute the config</param>
        /// <param name="config">The current config to execute</param>
        /// <returns>True, if successfully executed; Otherwise, False</returns>
        bool Execute(IJob job, IAppConfiguration config);
    }
}
