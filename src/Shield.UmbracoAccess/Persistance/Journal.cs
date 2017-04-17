using Newtonsoft.Json;

namespace Shield.UmbracoAccess.Persistance
{
    /// <summary>
    /// The Umbraco Access Journal serializable object for storing within the database.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Journal : Core.Persistance.Bal.IJsonValues
    {
        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        [JsonProperty]
        public string Message { get; set; }
    }
}
