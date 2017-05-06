using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.UmbracoAccess.Operation
{
    public class Operation : Core.Operation.Operation<Configuration>
    {
        public override string Id => nameof(Shield.UmbracoAccess);


        public Operation()
        {
        }

        public bool Execute(Configuration config)
        {
            return true;
        }
    }
}
