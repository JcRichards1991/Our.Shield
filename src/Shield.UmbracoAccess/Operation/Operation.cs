using System.Collections.Generic;

namespace Shield.UmbracoAccess.Operation
{
    public class Operation : Core.Operation.Operation<ViewModels.Configuration, IEnumerable<Core.Models.Journal>>
    {
        public override string Id => nameof(UmbracoAccess);

        public bool Execute(ViewModels.Configuration config)
        {
            return Module.Config(config);
        }
    }
}
