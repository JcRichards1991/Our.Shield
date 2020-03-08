namespace Our.Shield.Core.Settings
{
    public static class Configuration
    {
        private static IShieldSettings ShieldSettings => new ShieldSettings();

        public static string UmbracoPath => ApplicationSettings.UmbracoPath;

        public static string UmbracoVersion => ApplicationSettings.UmbracoVersion;

        public static IIpAddressValidation IpAddressValidation => ShieldSettings.IpAddressValidation;
    }
}
