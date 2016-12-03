using Shield.Persistance.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Persistance.UmbracoAccess
{
    public class UmbracoAccessConfiguration : IConfiguration
    {
        public string Name
        {
            get
            {
                return "UmbracoAccess";
            }
        }

        private string settings;
        public string Settings
        {
            get
            {
                return settings;
            }

            set
            {
                IsDirty = true;
                settings = value;
            }
        }

        public bool IsDirty { get; set; }

        public bool Save()
        {
            throw new NotImplementedException();
        }

        public IConfiguration Read()
        {
            throw new NotImplementedException();
        }
    }
}
