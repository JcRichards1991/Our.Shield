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

        public static class Tree
        {
            public const string Alias = nameof(Shield) + nameof(Tree);

            public const string Title = App.Name;
        }
    }
}
