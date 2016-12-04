using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace Shield.Persistance.Interfaces
{
    public interface IConfiguration
    {
        string Type { get; }

        DateTime Datestamp { get; set; }

        string Settings { get; set; }

        bool IsDirty { get; set; }

        bool Save();

        IConfiguration Read();
    }
}
