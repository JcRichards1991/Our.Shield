using Newtonsoft.Json;
using System;

namespace Shield.Persistance.UmbracoAccess
{
    /// <summary>
    /// The Umbraco Access Journal serializable object for storing within the database.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Journal : Bal.IJsonValues
    {
        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        [JsonProperty]
        public string Message { get; set; }
    }
}
