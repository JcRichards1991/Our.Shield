using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Persistance.Abstracts
{
    public abstract class Log : Interfaces.ILog
    {
        public int Id { get; set; }

        public virtual string Type { get; }

        public virtual string LogMessage { get; set; }
    }
}
