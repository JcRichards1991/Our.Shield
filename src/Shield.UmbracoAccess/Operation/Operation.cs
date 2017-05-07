namespace Shield.UmbracoAccess.Operation
{
    public class Operation : Core.Operation.Operation<Configuration>
    {
        public override string Id => nameof(UmbracoAccess);


        public Operation()
        {
        }

        public bool Execute(Configuration config)
        {
            return true;
        }
    }
}
