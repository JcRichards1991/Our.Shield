using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Controllers.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Our.Shield.Core.Models.Requests
{
    /// <summary>
    /// Model to represent the JSON for when posting an environment to the <see cref="ShieldApiController"/> <see cref="ShieldApiController.UpsertEnvironment"/> Endpoint
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy))]
    public class UpsertEnvironmentRequest
    {
        /// <summary>
        /// Key of the Environment
        /// </summary>
        [JsonProperty("key")]
        public Guid Key { get; set; } = default;

        /// <summary>
        /// Icon of the Environment
        /// </summary>
        [Required]
        [JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Name of the Environment
        /// </summary>
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The Domains the Environment responds too
        /// </summary>
        [JsonProperty("domains")]
        public IReadOnlyList<Domain> Domains { get; set; }

        /// <summary>
        /// Whether this Environment is enabled
        /// </summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Whether this environment should continue processing onto the next environment for handling requests
        /// </summary>
        [JsonProperty("continueProcessing")]
        public bool ContinueProcessing { get; set; }
    }
}
