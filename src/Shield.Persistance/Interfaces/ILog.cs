using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;

namespace Shield.Persistance.Interfaces
{
    public interface ILog
    {
        int Id { get; set; }

        string Type { get; }

        string LogMessage { get; set; }
    }
}
