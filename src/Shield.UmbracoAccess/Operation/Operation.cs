namespace Shield.UmbracoAccess.Operation
{
    public class Operation : Core.Operation.Operation<Configuration>
    {
        public override string Id => nameof(UmbracoAccess);

        public bool Execute(Configuration config)
        {
            return Module.Config(config);
        }
    }
}
