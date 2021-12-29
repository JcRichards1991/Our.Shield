namespace Our.Shield.Core.Settings
{
    /// <summary>
    /// 
    /// </summary>
    public static class ShieldConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        private static IShieldSettings ShieldSettings => new ShieldSettings();

        /// <summary>
        /// 
        /// </summary>
        public static string UmbracoPath => ApplicationSettings.UmbracoPath;

        /// <summary>
        /// 
        /// </summary>
        public static string UmbracoVersion => ApplicationSettings.UmbracoVersion;

        /// <summary>
        /// 
        /// </summary>
        public static IIpAddressValidation IpAddressValidation => ShieldSettings.IpAddressValidation;
    }
}
