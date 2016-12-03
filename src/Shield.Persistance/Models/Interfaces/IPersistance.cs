using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Persistance.Models.Interfaces
{
    public interface IPersistance
    {
        IEnumerable<IConfiguration> Configurations { get; set; }

        IEnumerable<ILog> Logs { get; set; }

        void Save();
    }
}
