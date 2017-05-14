namespace Shield.Core.Helpers
{
    using Umbraco.Web;
    using Umbraco.Web.PublishedCache;

    public static class UrlHelper
    {
        private static ContextualPublishedContentCache umbHelper = UmbracoContext.Current.ContentCache;

        public static string GetUrl(string xpath)
        {
            var node = umbHelper.GetSingleByXPath(xpath);

            if(node != null)
            {
                return node.Url;
            }

            return string.Empty;
        }

        public static string GetUrl(int id)
        {
            var node = umbHelper.GetById(id);

            if(node != null)
            {
                return node.Url;
            }

            return string.Empty;
        }
    }
}
