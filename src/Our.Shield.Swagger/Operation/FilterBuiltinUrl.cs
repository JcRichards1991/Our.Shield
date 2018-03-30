using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using Umbraco.Core.IO;

namespace Our.Shield.Swagger.Operation
{
	public class FilterBuiltinUrl : IDocumentFilter
    {
        /// <summary>
        /// Get the Umbraco Url
        /// </summary>
        public string UmbracoUrl
        {
            get
            {
                return IOHelper.ResolveUrl(Core.Settings.Configuration.UmbracoPath);
            }
        }

        /// <summary>
        /// Get the Installation Url
        /// </summary>
        public string InstallationUrl
        {
            get
            {
                return IOHelper.ResolveUrl(SystemDirectories.Install);
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
                .Where(entry => !entry.Key.StartsWith(UmbracoUrl, System.StringComparison.InvariantCultureIgnoreCase) && 
                    !entry.Key.StartsWith(InstallationUrl, System.StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }}
