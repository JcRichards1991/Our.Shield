namespace Our.Shield.Core.Models
{
    using System;

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
