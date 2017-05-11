namespace Shield.Core
{
    /// <summary>
    /// Container class for categories of constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Constants for the application
        /// </summary>
        public static class App
        {
            /// <summary>
            /// Custom section Name
            /// </summary>
            public const string Name = nameof(Shield);

            /// <summary>
            /// Custom section Alias
            /// </summary>
            public const string Alias = "shield";

            /// <summary>
            /// Custom section Icon
            /// </summary>
            public const string Icon = "icon-shield";
        }

        /// <summary>
        /// Contants for the tree controller
        /// </summary>
        public static class Tree
        {
            /// <summary>
            /// Tree Alias
            /// </summary>
            public const string Alias = nameof(Shield);

            /// <summary>
            /// Tree Title
            /// </summary>
            public const string Title = App.Name;

            /// <summary>
            /// Tree root node id
            /// </summary>
            public static string RootNodeId = Umbraco.Core.Constants.System.Root.ToString();
        }
    }
}
