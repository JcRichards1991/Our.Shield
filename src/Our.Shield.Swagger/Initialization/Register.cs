using Our.Shield.Swagger.DocumentFilters;
using Our.Shield.Swagger.Extensions;
using Swashbuckle.Application;
using System.Linq;
using System.Web.Http;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(Our.Shield.Swagger.Initialization.SwaggerConfig), nameof(Our.Shield.Swagger.Initialization.SwaggerConfig.PreStart))]
[assembly: PostApplicationStartMethod(typeof(Our.Shield.Swagger.Initialization.SwaggerConfig), nameof(Our.Shield.Swagger.Initialization.SwaggerConfig.PostStart))]

namespace Our.Shield.Swagger.Initialization
{
    public class SwaggerConfig
    {
        /// <summary>
        /// Entry point that is called on pre application startup
        /// </summary>
        public static void PreStart()
        {
            GlobalConfiguration.Configuration.SetSwaggerDocsConfig(c =>
            {
                c.SingleApiVersion("v1", "Website");
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.DocumentFilter<FilterBuiltinUrl>();
            });

            GlobalConfiguration.Configuration.SetSwaggerUiConfig(c =>
            {
                c.DisableValidator();
            });
        }

        /// <summary>
        /// Entry point that is called on post application startup
        /// </summary>
        public static void PostStart()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(GlobalConfiguration.Configuration.GetSwaggerDocsConfig())
                .EnableSwaggerUi(GlobalConfiguration.Configuration.GetSwaggerUiConfig());
        }
    }
}
