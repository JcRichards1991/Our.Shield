using Newtonsoft.Json;
using System.Collections.Generic;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Environment interface 
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// The Id of the Environment
        /// </summary>
        [JsonProperty("id")]
        int Id { get; }

        /// <summary>
        /// The Name of the Environment
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// The Icon of the Environment
        /// </summary>
        [JsonProperty("icon")]
        string Icon { get; }

        /// <summary>
        /// The Domains for the Environment
        /// </summary>
        [JsonProperty("domains")]
        IEnumerable<IDomain> Domains { get; }

        /// <summary>
        /// The sort order of the enviornment
        /// </summary>
        [JsonProperty("sortOrder")]
        int SortOrder { get; }

        /// <summary>
        /// whether or not the environment is enabled
        /// </summary>
        [JsonProperty("enable")]
        bool Enable { get; }

        /// <summary>
        /// Whether or not the environment should continue proccessing
        /// </summary>
        [JsonProperty("continueProcessing")]
        bool ContinueProcessing { get; }

        /// <summary>
        /// The Environment Color indicator as hexedecimal
        /// </summary>
        [JsonProperty("colorIndicator")]
        string ColorIndicator { get; }
    }
}
