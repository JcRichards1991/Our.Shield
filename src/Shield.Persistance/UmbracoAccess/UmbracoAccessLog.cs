using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Persistance.UmbracoAccess
{
    public class UmbracoAccessLog : Abstracts.Log
    {
        public override string Type
        {
            get
            {
                return "UmbracoAccess";
            }
        }
    }
}
