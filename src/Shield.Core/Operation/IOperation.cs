using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Operation
{
    public interface IOperation : IFrisk
    {
        bool Init();

        bool Execute(Configuration config);

        bool Write(bool enable, Configuration config);


    }
}
