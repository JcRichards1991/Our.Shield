using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Our.Shield.Core.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("Name: {Name}; Enabled: {Enabled}; Key: {Key}")]
    public class Environment : IEnvironment
    {
        /// <inheritdoc />
        public Guid Key { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Icon { get; set; }

        /// <inheritdoc />
        public IEnumerable<IDomain> Domains { get; set; }

        /// <inheritdoc />
        public int SortOrder { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public bool ContinueProcessing { get; set; }
    }
}
