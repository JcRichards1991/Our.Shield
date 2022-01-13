using Newtonsoft.Json;
using Our.Shield.Core.JsonConverters;
using System.Collections.Generic;

namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// Response for Getting Apps with their configuration by the requested Environment's key
    /// </summary>
    public class GetAppsResponse : BaseResponse
    {
        /// <summary>
        /// Apps with their configuration
        /// </summary>
        [JsonConverter(typeof(DictionaryAppConfigurationJsonConverter))]
        public IDictionary<IApp, IAppConfiguration> Apps { get; set; }
    }
}
