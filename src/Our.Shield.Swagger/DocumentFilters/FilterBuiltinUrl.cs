using System.Linq;
using System.Web.Http.Description;
using Our.Shield.Core.Settings;
using Swashbuckle.Swagger;
using Umbraco.Core.IO;

namespace Our.Shield.Swagger.DocumentFilters
{
    /// <summary>
    /// Filters out Umbraco's Install, Backoffice, CanvasDesigner and Tags Api's
    /// </summary>
	public class FilterBuiltinUrl : IDocumentFilter
    {
        private static string _umbracoUrl { get; set; }

        /// <summary>
        /// Get the Umbraco Url
        /// </summary>
        private string UmbracoUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_umbracoUrl))
                    return _umbracoUrl;

                return _umbracoUrl = IOHelper.ResolveUrl(ShieldConfiguration.UmbracoPath);
            }
        }

        private static string _umbracoBackofficeUrl { get; set; }

        /// <summary>
        /// Get the Umbraco Backoffice Url
        /// </summary>
        private string UmbracoBackofficeUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_umbracoBackofficeUrl))
                    return _umbracoBackofficeUrl;

                return _umbracoBackofficeUrl = IOHelper.ResolveUrl($"{UmbracoUrl}backoffice");
            }
        }

        private static string _installationUrl { get; set; }

        /// <summary>
        /// Get the Installation Url
        /// </summary>
        private string InstallationUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_installationUrl))
                    return _installationUrl;

                return _installationUrl = IOHelper.ResolveUrl(SystemDirectories.Install);
            }
        }

        private static string _canvasDesignerUrl { get; set; }

        /// <summary>
        /// Get the Installation Url
        /// </summary>
        private string CanvasDesignerUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_canvasDesignerUrl))
                    return _canvasDesignerUrl;

                return _canvasDesignerUrl = IOHelper.ResolveUrl($"{UmbracoUrl}Api/CanvasDesigner");
            }
        }

        private static string _tagsUrl { get; set; }

        /// <summary>
        /// Get the Installation Url
        /// </summary>
        private string TagsUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_tagsUrl))
                    return _tagsUrl;

                return _tagsUrl = IOHelper.ResolveUrl($"{UmbracoUrl}Api/Tags");
            }
        }

        /// <summary>
        /// Apply method of IDocumentFilter that is called to filter routes
        /// </summary>
        /// <param name="swaggerDoc">Contains the swagger request</param>
        /// <param name="schemaRegistry">Swagger config settings</param>
        /// <param name="apiExplorer">Description of routes</param>
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            swaggerDoc.paths = swaggerDoc.paths
                .Where(entry => !entry.Key.StartsWith(UmbracoBackofficeUrl, System.StringComparison.InvariantCultureIgnoreCase) &&
                    !entry.Key.StartsWith(InstallationUrl, System.StringComparison.InvariantCultureIgnoreCase) &&
                    !entry.Key.StartsWith(CanvasDesignerUrl, System.StringComparison.InvariantCultureIgnoreCase) &&
                    !entry.Key.StartsWith(TagsUrl, System.StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }
}
