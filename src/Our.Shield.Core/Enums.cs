namespace Our.Shield.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Enums
    {
        /// <summary>
        /// Whether or not the user should be redirected or rewritten
        /// </summary>
        public enum UnauthorisedAction
        {
            Redirect,
            Rewrite
        }

        /// <summary>
        /// Whether IP Address restrictions is enabled or disabled
        /// </summary>
        public enum IpAddressesAccess
        {
            Unrestricted,
            Restricted
        }

        /// <summary>
        /// The selector for getting the Url
        /// </summary>
        public enum UrlType
        {
            Url,
            XPath,
            ContentPicker
        }
    }
}
