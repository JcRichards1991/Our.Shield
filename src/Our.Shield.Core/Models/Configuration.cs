using System;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// The Base class for an App's Configuration
    /// </summary>
    public abstract class Configuration : IConfiguration
    {
        /// <summary>
        /// Gets or sets whether the Configuration is Enabled.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the configuration last modified data time
        /// </summary>
        public DateTime? LastModified { get; internal set; }
    }
}
