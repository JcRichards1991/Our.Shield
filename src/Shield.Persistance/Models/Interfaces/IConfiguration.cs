using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Persistance.Models.Interfaces
{
    public interface IConfiguration
    {
        string Name { get; }

        string Settings { get; set; }

        bool IsDirty { get; set; }

        bool Save();

        IConfiguration Read();
    }
}
