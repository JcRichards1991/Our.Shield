namespace Shield.UmbracoAccess
{
    public class Constants
    {
        public class Tree
        {
            public const string Title = nameof(Shield);
            public const string Alias = nameof(UmbracoAccess);
            public const string NodeName = "Umbraco Access";
            public const string NodeId = "-313131";
            public static readonly string RootNodeId = Umbraco.Core.Constants.System.Root.ToString();
        }

        public class Defaults
        {
            public const string BackendAccessUrl = "/umbraco";
        }
    }
}
