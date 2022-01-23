using System;
using System.Diagnostics;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// Implements <see cref="IAppConfiguration"/> for the required properties for configurations
    /// </summary>
    [DebuggerDisplay("Enable: {Enabled}")]
    public abstract class AppConfiguration : IAppConfiguration
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public DateTime? LastModifiedDateUtc { get; set; }
    }
}
