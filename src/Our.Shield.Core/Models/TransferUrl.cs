using Newtonsoft.Json;
using System.ComponentModel;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Transfer types
    /// </summary>
    public enum TransferTypes
    {
        [Description("Stops all further communication with this session. The client will be left hanging, they will conclude your server is unreachable/invalid/unresponsive")]
        PlayDead = -1,

        [Description("Informs the client of a new Url to use. Their address bar will be updated to reflect this new Url")]
        Redirect = 0,

        [Description("Internally restarts the process with a new Url, but their Address bar will not reflect this change")]
        Rewrite = 1
    }

    public class TransferUrl
    {
        /// <summary>
        /// What action do we take for this Url
        /// </summary>
        [JsonProperty("transferType")]
        public TransferTypes TransferType { get; set; }

        /// <summary>
        /// The value of the Url
        /// </summary>
        [JsonProperty("url")]
        public UmbracoUrl Url { get; set; }
    }
}
