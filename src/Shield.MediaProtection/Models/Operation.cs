namespace Shield.MediaProtection.Models
{
    public class Operation : Core.Models.Operation<ViewModels.Configuration>
    {
        public override string Id => nameof(MediaProtection);

        public override bool Init()
        {
            return false;
        }

        public bool Execute(ViewModels.Configuration config)
        {
            return true;
        }
    }
}
