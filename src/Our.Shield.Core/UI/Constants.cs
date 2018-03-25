namespace Our.Shield.Core.UI
{
    /// <summary>
    /// Container class for categories of constants
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Constants for the application
        /// </summary>
        public class App
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
        /// Constants relating to the dashboards of shield
        /// </summary>
        public class Dashboard
        {
            /// <summary>
            /// The Environment Node Root Id
            /// </summary>
            public const int EnvironmentsDashboardId = 0;
        }

        /// <summary>
        /// Constants for the tree controller
        /// </summary>
        public class Tree
        {
            /// <summary>
            /// The default environment id
            /// </summary>
            public const int DefaultEnvironmentSortOrder = 999999;

            public const int DefaultEnvironmentId = 1;

            public const int CreateEnvironmentId = -100;
        }
    }
}
