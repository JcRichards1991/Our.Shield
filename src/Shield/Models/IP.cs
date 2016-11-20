using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Models
{
    public class IP
    {
        public string IPAddress { get; set; }

        public string Name { get; set; }

        public Enums.Command AllowDeny { get; set; }
    }
}
