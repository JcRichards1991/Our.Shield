using Newtonsoft.Json;
using System;

namespace Shield.Persistance.UmbracoAccess
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Journal : Bal.JsonValues
    {
        [JsonProperty]
        public string Message { get; set; }
    }
}
