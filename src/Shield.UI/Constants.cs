namespace Shield.UI
{
    public class Constants
    {
        public class App
        {
            public const string Name = "Shield";
            public const string Alias = "shield";
            public const string Icon = "icon-shield";
        }
        
        public class Tree
        {
            public class UmbracoAccess
            {
                public const string Title = "Shield";
                public const string Alias = nameof(UmbracoAccess);
                public const string NodeName = "Umbraco Access";
                public const string NodeId = "-313131";
            }
        }

        public static readonly string RootNodeId = global::Umbraco.Core.Constants.System.Root.ToString();

        public class Defaults
        {
            public const string BackendAccessUrl = "~/umbraco";
            public const int StatusCode = 404;
        }
    }
}
