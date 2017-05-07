using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.UmbracoAccess.Operation
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Journal : Core.Operation.Journal
    {
        /// <summary>
        /// Gets or sets the Message.
        /// </summary>
        [JsonProperty]
        public string Message { get; set; }
    }
}
