using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Our.Shield.Core.Controllers.Api;
using System.Collections.Generic;

namespace Our.Shield.Core.Models.Requests
{
    /// <summary>
    /// Model to represent the JSON for when posting an environment to the <see cref="ShieldApiController"/> <see cref="ShieldApiController.SortEnvironments"/> Endpoint
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy))]
    public class UpdateEnvironmentsSortOrderRequest
    {
        /// <summary>
        /// The Environments in the correct sort order
        /// </summary>
        [JsonProperty("environments")]
        public IEnumerable<Environment> Environments { get; set; }
    }
}
