namespace Our.Shield.BackofficeAccess
{
    /// <summary>
    /// 
    /// </summary>
    public class Enums
    {
        /// <summary>
        /// Whether or not the user should be Redirected or Rewrite to the unauthorised Url
        /// </summary>
        public enum RedirectRewrite
        {
            Redirect,
            Rewrite
        }

        /// <summary>
        /// The selector for getting the unauthorised Url
        /// </summary>
        public enum UnautorisedUrlType
        {
            Url,
            XPath,
            ContentPicker
        }
    }
}
