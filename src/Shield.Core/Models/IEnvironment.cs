using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Models
{
    public interface IEnvironment
    {
        int Id { get; }

        string Name { get; }

        IEnumerable<IDomain> Domains { get; }
    }
}
