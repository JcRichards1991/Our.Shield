using System;
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
        /// The unique identifier of the <see cref="IEnvironment"/>
        /// </summary>
        [JsonProperty("key")]
        Guid Key { get; }

        /// <summary>
        /// The name of the <see cref="IEnvironment"/>
        /// </summary>
        [JsonProperty("name")]
        string Name { get; }

        /// <summary>
        /// The icon of the <see cref="IEnvironment"/>
        /// </summary>
        [JsonProperty("icon")]
        string Icon { get; }

        /// <summary>
        /// The domains the <see cref="IEnvironment"/> handles requests for
        /// </summary>
        [JsonProperty("domains")]
        IEnumerable<IDomain> Domains { get; }

        /// <summary>
        /// The sort order of the <see cref="IEnvironment"/> that appears in the UI and the order in which the environments will be processed in
        /// </summary>
        [JsonProperty("sortOrder")]
        int SortOrder { get; }

        /// <summary>
        /// whether or not the <see cref="IEnvironment"/> is enabled, and sub-sequently the environment's app(s)
        /// </summary>
        [JsonProperty("enable")]
        bool Enabled { get; }

        /// <summary>
        /// Whether or not the <see cref="IEnvironment"/> should continue processing to the next <see cref="IEnvironment"/> to handle if applicable
        /// </summary>
        [JsonProperty("continueProcessing")]
        bool ContinueProcessing { get; }
    }
}
