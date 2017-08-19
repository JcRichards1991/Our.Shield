namespace Our.Shield.BackofficeAccess.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Enums
    {
        /// <summary>
        /// The selector for getting the unauthorised Url
        /// </summary>
        public enum UnautorisedUrlType
        {
            Url,
            XPath,
            ContentPicker
        }

        /// <summary>
        /// 
        /// </summary>
        public enum IpAddressesRestricted
        {
            Unrestricted,
            Restricted
        }
    }
}
