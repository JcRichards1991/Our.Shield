namespace Shield.MediaProtection.Models
{
    public class Operation : Core.Models.Operation<ViewModels.Configuration>
    {
        public override string Id => nameof(MediaProtection);

        public bool Execute(ViewModels.Configuration config)
        {
            return true;
        }
    }
}
