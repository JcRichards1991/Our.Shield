using Shield.Persistance.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Persistance.UmbracoAccess
{
    public class UmbracoAccessConfiguration : Models.Configuration
    {
        public override string Type
        {
            get
            {
                return "UmbracoAccess";
            }
        }

        public override bool Save()
        {
            throw new NotImplementedException();
        }

        public override IConfiguration Read()
        {
            throw new NotImplementedException();
        }
    }
}
