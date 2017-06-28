using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Models
{
    public abstract class Journal : IJournal
    {
        public string AppId { get; internal set; }
        public int EnvironmentId { get; }
        public DateTime Datestamp { get; }
        

    }
}
