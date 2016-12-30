using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Persistance.Abstracts
{
    public class Persistance : Interfaces.IPersistance
    {
        public IEnumerable<Interfaces.IConfiguration> Configurations { get; set; }

        public IEnumerable<Interfaces.ILog> Logs { get; set; }

        public void Save()
        {
            foreach(var item in Configurations)
            {
                item.Save();
            }
        }
    }
}
