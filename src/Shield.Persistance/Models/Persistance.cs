using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.Persistance.Models.Interfaces;

namespace Shield.Persistance.Models
{
    public class Persistance : IPersistance
    {
        public IEnumerable<IConfiguration> Configurations { get; set; }

        public IEnumerable<ILog> Logs { get; set; }

        public void Save()
        {
            foreach(var item in Configurations)
            {
                item.Save();
            }
        }
    }
}
