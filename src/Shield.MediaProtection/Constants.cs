namespace Shield.MediaProtection
{
    public class Constants
    {
        public class Tree
        {
            public const string Title = nameof(Shield);
            public const string Alias = nameof(MediaProtection);
            public const string NodeName = "Media Protection";
            public const string NodeId = "-313132";
            public static readonly string RootNodeId = Umbraco.Core.Constants.System.Root.ToString();
        }

        public class Defaults
        {
            public const string BackendAccessUrl = "/umbraco";
        }
    }
}
