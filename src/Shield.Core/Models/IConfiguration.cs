using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Models
{
    public interface IConfiguration
    {
        /// <summary>
        /// Gets or sets what environment this is for
        /// </summary>
        IEnvironment Environment { get; }

        /// <summary>
        /// Gets or sets what shield app is this for
        /// </summary>
        IApp App { get; }

        /// <summary>
        /// Gets or sets whether the Configuration is Enabled.
        /// </summary>
        bool Enable { get; set; }

        /// <summary>
        /// Gets or sets the configuration last modified data time
        /// </summary>
        DateTime? LastModified { get; }
    }
}
