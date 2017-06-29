using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Shield.Core.Models
{
    public abstract class Configuration : IConfiguration
    {
        /// <summary>
        /// Gets or sets what environment this is for
        /// </summary>
        [JsonIgnore]
        public IEnvironment Environment { get; internal set; }

        /// <summary>
        /// Gets or sets what shield app is this for
        /// </summary>
        [JsonIgnore]
        public IApp App { get; internal set; }
        
        /// <summary>
        /// Gets or sets whether the Configuration is Enabled.
        /// </summary>
        [JsonIgnore]
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the configuration last modified data time
        /// </summary>
        [JsonIgnore]
        public DateTime? LastModified { get; internal set; }

    }
}
