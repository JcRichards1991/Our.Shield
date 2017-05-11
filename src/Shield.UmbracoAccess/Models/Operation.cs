namespace Shield.UmbracoAccess.Models
{
    public class Operation : Core.Models.Operation<ViewModels.Configuration>
    {
        public override string Id => nameof(UmbracoAccess);

        public bool Execute(ViewModels.Configuration config)
        {
            return true;
        }
    }
}
