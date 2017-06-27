using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Models
{
    internal class Domain : IDomain
    {
        public string Name { get; set; }

        internal Domain(Persistance.Data.Dto.Domain data)
        {
            Name = data.Name;
        }

        public override bool Equals(object other)
        {
            var otherDomain = other as Domain;
            if (otherDomain == null)
            {
                return false;
            }
            return Name == otherDomain.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

    }
}
