using System.Collections.Generic;

namespace Shield.UmbracoAccess.Operation
{
    public class Operation : Core.Models.Operation<ViewModels.Configuration>
    {
        public override string Id => nameof(UmbracoAccess);

        public bool Execute(ViewModels.Configuration config)
        {
            return Module.Config(config);
        }
    }
}
