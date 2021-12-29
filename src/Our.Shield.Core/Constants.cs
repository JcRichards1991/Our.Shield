namespace Our.Shield.Core
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
        /// Constants for the Distributed Cache
        /// </summary>
        public class DistributedCache
        {
            /// <summary>
            /// Key of the Environment Cache Refresher
            /// </summary>
            public const string EnvironmentCacheRefresherId = "2d3d8e66-7a63-405a-aa13-24095f0bacb5";

            /// <summary>
            /// Key of the Configuration Cache Refresher
            /// </summary>
            public const string ConfigurationCacheRefresherId = "04cf399a-8d7d-4b1c-8320-9020f4e34c91";
        }
    }
}
